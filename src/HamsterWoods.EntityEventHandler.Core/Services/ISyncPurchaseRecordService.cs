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
using HamsterWoods.Rank.Provider;
using HamsterWoods.Trace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.EntityEventHandler.Core.Services;

public interface ISyncPurchaseRecordService
{
    Task SyncPurchaseRecordAsync();
}

public class SyncPurchaseRecordService : ISyncPurchaseRecordService, ISingletonDependency
{
    private readonly ILogger<SyncPurchaseRecordService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IGraphQLHelper _graphQlHelper;
    private readonly INESTRepository<PurchaseRecordIndex, string> _repository;
    private readonly INESTRepository<PointsInfoIndex, string> _pointsInfoRepository;
    private readonly IClusterClient _clusterClient;
    private readonly ICacheProvider _cacheProvider;
    private const string PurchaseEndTimeCacheKey = "PurchaseEndTime";
    private readonly IOptionsMonitor<PointsTaskOptions> _options;
    private readonly IRankProvider _rankProvider;

    public SyncPurchaseRecordService(ILogger<SyncPurchaseRecordService> logger, IObjectMapper objectMapper,
        IGraphQLHelper graphQlHelper, INESTRepository<PurchaseRecordIndex, string> repository,
        INESTRepository<PointsInfoIndex, string> pointsInfoRepository, IClusterClient clusterClient,
        ICacheProvider cacheProvider, IOptionsMonitor<PointsTaskOptions> options, IRankProvider rankProvider)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _graphQlHelper = graphQlHelper;
        _repository = repository;
        _pointsInfoRepository = pointsInfoRepository;
        _clusterClient = clusterClient;
        _cacheProvider = cacheProvider;
        _options = options;
        _rankProvider = rankProvider;
    }

    public async Task SyncPurchaseRecordAsync()
    {
        var raceInfo = await _rankProvider.GetCurrentRaceInfoAsync();
        var queryTime = await GeQueryTimeAsync(raceInfo.CurrentRaceTimeInfo.BeginTime, raceInfo.WeekNum);
        var beginTime = queryTime.Item1;
        var endTime = queryTime.Item2;
        var weekNum = queryTime.Item3;

        _logger.LogInformation("[SyncPurchaseRecord] start, beginTime:{beginTime}, endTime:{endTime}", beginTime,
            endTime);

        var records = new List<PurchaseResultDto>();
        var recordIndices = new List<PurchaseRecordIndex>();
        var skipCount = 0;

        records = await GetPurchaseRecordAsync(beginTime, endTime, skipCount,
            CommonConstant.DefaultQueryMaxResultCount);
        if (records.IsNullOrEmpty()) return;

        recordIndices.AddRange(_objectMapper.Map<List<PurchaseResultDto>, List<PurchaseRecordIndex>>(records));
        while (!records.IsNullOrEmpty() && records.Count == CommonConstant.DefaultQueryMaxResultCount)
        {
            skipCount += CommonConstant.DefaultQueryMaxResultCount;
            records = await GetPurchaseRecordAsync(beginTime, endTime, skipCount,
                CommonConstant.DefaultQueryMaxResultCount);

            if (!records.IsNullOrEmpty())
            {
                recordIndices.AddRange(_objectMapper.Map<List<PurchaseResultDto>, List<PurchaseRecordIndex>>(records));
            }
        }

        var cacheEndTime = recordIndices.Max(t => t.TriggerTime);

        foreach (var group in recordIndices.GroupBy(t => t.CaAddress))
        {
            try
            {
                var grain = _clusterClient.GetGrain<IUserPointsGrain>(AddressHelper.ToShortAddress(group.Key));
                var chanceInfos = group.Select(item => new ChanceInfo { Id = item.Id, ChanceCount = item.Chance })
                    .ToList();
                var resultDto = await grain.SePurchase(weekNum, chanceInfos);
                if (!resultDto.Success)
                {
                    _logger.LogError("[SyncPurchaseRecord] set chance info success, message:{message}, data:{data}",
                        resultDto.Message, JsonConvert.SerializeObject(group));
                }
                
                var countInfo = resultDto.Data;
                if (countInfo.LastCount == countInfo.CurrentCount) continue;

                var amount = GetAmount(countInfo.LastCount, countInfo.CurrentCount);
                _logger.LogInformation(
                    "[SyncPurchaseRecord] address:{address}, lastCount:{lastCount}, currentCount:{currentCount}, amount:{amount}",
                    group.Key, countInfo.LastCount, countInfo.CurrentCount, amount);
                if (amount == 0) continue;

                var grainId = Guid.NewGuid().ToString();
                var pointAmount = AmountHelper.GetAmount(amount, 8);
                var pointsInfoGrain = _clusterClient.GetGrain<IPointsInfoGrain>(grainId);
                var grainDto = await pointsInfoGrain.Create(new PointsInfoGrainDto()
                {
                    Address = AddressHelper.ToShortAddress(group.Key),
                    Amount = pointAmount,
                    PointType = PointType.PurchaseCount
                });

                await _pointsInfoRepository.AddOrUpdateAsync(
                    _objectMapper.Map<PointsInfoGrainDto, PointsInfoIndex>(grainDto.Data));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[SyncPurchaseRecord] set purchase chance and cal error, data:{data}",
                    JsonConvert.SerializeObject(group));
            }
        }
        
        await _repository.BulkAddOrUpdateAsync(recordIndices);
        await _cacheProvider.Set(PurchaseEndTimeCacheKey, new SyncRecordTimeCache()
        {
            LastSyncTime = cacheEndTime.AddMilliseconds(1)
        }, null);
        _logger.LogInformation(
            "[SyncPurchaseRecord] end, beginTime:{beginTime}, endTime:{endTime}, syncCount:{syncCount}, cacheEndTime:{cacheEndTime}",
            beginTime, endTime, recordIndices.Count, cacheEndTime);
    }

    private int GetAmount(int lastCount, int currentCount)
    {
        var lastAmount = GetAmount(lastCount);
        var currentAmount = GetAmount(currentCount);
        return currentAmount - lastAmount;
    }

    private int GetAmount(int chanceCount)
    {
        var amount = 0;
        var configs = _options.CurrentValue.Chance.ChanceConfigs;
        foreach (var config in configs.OrderBy(t => t.FromCount))
        {
            if (chanceCount < config.FromCount) break;

            if (chanceCount <= config.ToCount)
            {
                amount += config.PointAmount * (chanceCount - config.FromCount + 1);
                continue;
            }

            amount += config.PointAmount * (config.ToCount - config.FromCount + 1);
        }

        return amount;
    }

    private async Task<Tuple<DateTime, DateTime, int>> GeQueryTimeAsync(DateTime raceStartTime, int weekNum)
    {
        var timeInfo = await _cacheProvider.Get<SyncRecordTimeCache>(PurchaseEndTimeCacheKey);
        var startTime = raceStartTime;
        if (timeInfo != null)
        {
            startTime = timeInfo.LastSyncTime;
        }

        var endTime = DateTime.UtcNow;
        if (timeInfo != null && timeInfo.LastSyncTime.Date < raceStartTime.Date)
        {
            endTime = raceStartTime.Date;
            weekNum -= 1;
        }

        return new Tuple<DateTime, DateTime, int>(startTime, endTime, weekNum);
    }


    private async Task<List<PurchaseResultDto>> GetPurchaseRecordAsync(DateTime beginTime, DateTime endTime,
        int skipCount,
        int maxResultCount)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<GetPurchaseRecordResultDto>(new GraphQLRequest
        {
            Query = @"
			    query($startTime:DateTime!,$endTime:DateTime!,$skipCount:Int!,$maxResultCount:Int!) {
                  getPurchaseRecords(requestDto:{startTime:$startTime,endTime:$endTime,skipCount:$skipCount,maxResultCount:$maxResultCount}){
                    buyChanceList{
                      id
                      caAddress
                      cost
                      chance
                      transactionInfo{
                        triggerTime,
                        transactionId
                      }
                    }
                }
            }",
            Variables = new
            {
                startTime = beginTime,
                endTime = endTime,
                skipCount = skipCount,
                maxResultCount = maxResultCount
            }
        });
        var result = graphQLResponse?.GetPurchaseRecords?.BuyChanceList;
        return result ?? new List<PurchaseResultDto>();
    }
}