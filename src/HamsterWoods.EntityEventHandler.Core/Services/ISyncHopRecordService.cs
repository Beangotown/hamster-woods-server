using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using GraphQL;
using HamsterWoods.Cache;
using HamsterWoods.Common;
using HamsterWoods.Commons;
using HamsterWoods.Enums;
using HamsterWoods.Grains.Grain.Points;
using HamsterWoods.Grains.Grain.UserPoints;
using HamsterWoods.Options;
using HamsterWoods.Points;
using HamsterWoods.Points.Dtos;
using HamsterWoods.Trace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.EntityEventHandler.Core.Services;

public interface ISyncHopRecordService
{
    Task SyncHopRecordAsync();
}

public class SyncHopRecordService : ISyncHopRecordService, ISingletonDependency
{
    private readonly ILogger<SyncHopRecordService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IGraphQLHelper _graphQlHelper;
    private readonly INESTRepository<HopRecordIndex, string> _repository;
    private readonly INESTRepository<PointsInfoIndex, string> _pointsInfoRepository;
    private readonly IClusterClient _clusterClient;
    private readonly ICacheProvider _cacheProvider;
    private const string HopEndTimeCacheKey = "HopEndTime";
    private readonly IOptionsMonitor<PointsTaskOptions> _options;

    public SyncHopRecordService(IGraphQLHelper graphQlHelper, ILogger<SyncHopRecordService> logger,
        IObjectMapper objectMapper, INESTRepository<HopRecordIndex, string> repository, IClusterClient clusterClient,
        INESTRepository<PointsInfoIndex, string> pointsInfoRepository, ICacheProvider cacheProvider,
        IOptionsMonitor<PointsTaskOptions> options)
    {
        _graphQlHelper = graphQlHelper;
        _logger = logger;
        _objectMapper = objectMapper;
        _repository = repository;
        _clusterClient = clusterClient;
        _pointsInfoRepository = pointsInfoRepository;
        _cacheProvider = cacheProvider;
        _options = options;
    }

    public async Task SyncHopRecordAsync()
    {
        var queryTime = await GeQueryTimeAsync();
        var beginTime = queryTime.Item1;
        var endTime = queryTime.Item2;
        _logger.LogInformation("[SyncHopRecord] start, beginTime:{beginTime}, endTime:{endTime}", beginTime, endTime);

        var records = new List<GameResultDto>();
        var hopRecordIndices = new List<HopRecordIndex>();
        var skipCount = 0;

        records = await GetHopRecordAsync(beginTime, endTime, skipCount,
            CommonConstant.DefaultQueryMaxResultCount);
        if (records.IsNullOrEmpty()) return;

        hopRecordIndices.AddRange(_objectMapper.Map<List<GameResultDto>, List<HopRecordIndex>>(records));

        while (!records.IsNullOrEmpty() && records.Count == CommonConstant.DefaultQueryMaxResultCount)
        {
            skipCount += CommonConstant.DefaultQueryMaxResultCount;
            records = await GetHopRecordAsync(beginTime, endTime, skipCount,
                CommonConstant.DefaultQueryMaxResultCount);

            if (!records.IsNullOrEmpty())
            {
                hopRecordIndices.AddRange(_objectMapper.Map<List<GameResultDto>, List<HopRecordIndex>>(records));
            }
        }

        var cacheEndTime = hopRecordIndices.Max(t => t.TriggerTime);
        foreach (var hopGroup in hopRecordIndices.GroupBy(t => t.CaAddress))
        {
            try
            {
                var grain = _clusterClient.GetGrain<IUserPointsGrain>(AddressHelper.ToShortAddress(hopGroup.Key));
                var ids = hopGroup.Select(item => item.Id).ToList();
                var resultDto = await grain.SetHop(ids);
                if (!resultDto.Success)
                {
                    _logger.LogError("[SyncHopRecord] set hop not success, message:{message}, data:{data}",
                        resultDto.Message, JsonConvert.SerializeObject(hopGroup));
                }
                
                var countInfo = resultDto.Data;
                if (countInfo.LastCount == countInfo.CurrentCount) continue;

                var amount = GetAmount(countInfo.LastCount, countInfo.CurrentCount);
                _logger.LogInformation(
                    "[SyncHopRecord] address:{address}, lastCount:{lastCount}, currentCount:{currentCount}, amount:{amount}",
                    hopGroup.Key, countInfo.LastCount, countInfo.CurrentCount, amount);
                if (amount == 0) continue;

                var grainId = Guid.NewGuid().ToString();
                var pointAmount = AmountHelper.GetAmount(amount, 8);
                var pointsInfoGrain = _clusterClient.GetGrain<IPointsInfoGrain>(grainId);
                var grainDto = await pointsInfoGrain.Create(new PointsInfoGrainDto()
                {
                    Address = AddressHelper.ToShortAddress(hopGroup.Key),
                    Amount = pointAmount,
                    PointType = PointType.Hop
                });

                await _pointsInfoRepository.AddOrUpdateAsync(
                    _objectMapper.Map<PointsInfoGrainDto, PointsInfoIndex>(grainDto.Data));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[SyncHopRecord] set hop and cal error, data:{data}",
                    JsonConvert.SerializeObject(hopGroup));
            }
        }

        await _repository.BulkAddOrUpdateAsync(hopRecordIndices);
        await _cacheProvider.Set(HopEndTimeCacheKey, new SyncRecordTimeCache()
        {
            LastSyncTime = cacheEndTime.AddMilliseconds(1)
        }, null);
        _logger.LogInformation(
            "[SyncHopRecord] end, beginTime:{beginTime}, endTime:{endTime}, syncCount:{syncCount}, cacheEndTime:{cacheEndTime}",
            beginTime, endTime, hopRecordIndices.Count, cacheEndTime);
    }

    private async Task<Tuple<DateTime, DateTime>> GeQueryTimeAsync()
    {
        var timeInfo = await _cacheProvider.Get<SyncRecordTimeCache>(HopEndTimeCacheKey);
        var startTime = DateTime.UtcNow.Date;
        if (timeInfo != null)
        {
            startTime = timeInfo.LastSyncTime;
        }

        var endTime = DateTime.UtcNow;

        if (startTime.Date < endTime.Date)
        {
            endTime = DateTime.Now.Date;
        }

        return new Tuple<DateTime, DateTime>(startTime, endTime);
    }

    private int GetAmount(int lastCount, int currentCount)
    {
        var lastAmount = GetAmount(lastCount);
        var currentAmount = GetAmount(currentCount);
        return currentAmount - lastAmount;
    }

    private int GetAmount(int hopCount)
    {
        var amount = 0;
        var configs = _options.CurrentValue.Hop.HopConfigs;
        foreach (var config in configs.OrderBy(t => t.HopCount))
        {
            if (hopCount < config.HopCount) break;

            if (config.IsOverHop)
            {
                amount += config.PointAmount * (hopCount - config.HopCount);
                continue;
            }

            amount += config.PointAmount;
        }

        return amount;
    }

    private async Task<List<GameResultDto>> GetHopRecordAsync(DateTime beginTime, DateTime endTime, int skipCount,
        int maxResultCount)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<GameHistoryResultDto>(new GraphQLRequest
        {
            Query = @"
			    query($beginTime:DateTime!,$endTime:DateTime!,$skipCount:Int!,$maxResultCount:Int!) {
                  getGameHistoryList(getGameHistoryDto:{beginTime:$beginTime,endTime:$endTime,skipCount:$skipCount,maxResultCount:$maxResultCount}){
                    gameList{
                      id
                      caAddress
                      bingoTransactionInfo{
                        triggerTime,
                        transactionId
                      }
                    }
                }
            }",
            Variables = new
            {
                beginTime = beginTime,
                endTime = endTime,
                skipCount = skipCount,
                maxResultCount = maxResultCount
            }
        });
        var result = graphQLResponse?.GetGameHistoryList?.GameList;
        return result ?? new List<GameResultDto>();
    }
}