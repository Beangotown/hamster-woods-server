using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using GraphQL;
using HamsterWoods.Common;
using HamsterWoods.Commons;
using HamsterWoods.Enums;
using HamsterWoods.Grains.Grain.UserPoints;
using HamsterWoods.Points;
using HamsterWoods.Trace;
using Microsoft.Extensions.Logging;
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

    public SyncHopRecordService(IGraphQLHelper graphQlHelper, ILogger<SyncHopRecordService> logger,
        IObjectMapper objectMapper, INESTRepository<HopRecordIndex, string> repository, IClusterClient clusterClient,
        INESTRepository<PointsInfoIndex, string> pointsInfoRepository)
    {
        _graphQlHelper = graphQlHelper;
        _logger = logger;
        _objectMapper = objectMapper;
        _repository = repository;
        _clusterClient = clusterClient;
        _pointsInfoRepository = pointsInfoRepository;
    }

    public async Task SyncHopRecordAsync()
    {
        var beginTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
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

                // cal amount
                var countInfo = resultDto.Data;
                if (countInfo.LastCount == countInfo.CurrentCount) continue;

                var currentCount = countInfo.CurrentCount;
                var amount = GetAmount(countInfo.LastCount, countInfo.CurrentCount);
                //var count = countInfo.CurrentCount - countInfo.LastCount;

                // create order
                await _pointsInfoRepository.AddOrUpdateAsync(new PointsInfoIndex()
                {
                    Id = Guid.NewGuid().ToString(),
                    Address = AddressHelper.ToShortAddress(hopGroup.Key),
                    Amount = amount * 100000000,
                    ContractInvokeStatus = "None",
                    PointType = PointType.Hop.ToString(),
                    CreateTime = DateTime.UtcNow
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[SyncHopRecord] set hop and cal error, data:{data}",
                    JsonConvert.SerializeObject(hopGroup));
            }
        }

        await _repository.BulkAddOrUpdateAsync(hopRecordIndices);
        _logger.LogInformation("[SyncHopRecord] end, beginTime:{beginTime}, endTime:{endTime}, syncCount:{syncCount}",
            beginTime, endTime, hopRecordIndices.Count);
    }

    private int GetAmount(int lastCount, int currentCount)
    {
        var amount = 0;
        if (lastCount < 1)
        {
            if (currentCount >= 1)
            {
                amount += 3;
            }

            if (currentCount >= 5)
            {
                amount += 20;
            }

            if (currentCount >= 7)
            {
                amount += 35;
            }

            if (currentCount >= 10)
            {
                amount += 60;
            }

            if (currentCount >= 15)
            {
                amount += 100;
            }
        }

        else if (lastCount < 5)
        {
            if (currentCount >= 5)
            {
                amount += 20;
            }

            if (currentCount >= 7)
            {
                amount += 35;
            }

            if (currentCount >= 10)
            {
                amount += 60;
            }

            if (currentCount >= 15)
            {
                amount += 100;
            }
        }
        else if (lastCount < 7)
        {
            if (currentCount >= 7)
            {
                amount += 35;
            }

            if (currentCount >= 10)
            {
                amount += 60;
            }

            if (currentCount >= 15)
            {
                amount += 100;
            }
        }
        else if (lastCount < 10)
        {
            if (currentCount >= 10)
            {
                amount += 60;
            }

            if (currentCount >= 15)
            {
                amount += 100;
            }
        }

        return amount;
    }

    private async Task<List<GameResultDto>> GetHopRecordAsync(DateTime beginTime, DateTime endTime, int skipCount,
        int maxResultCount)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<GameHistoryResultDto>(new GraphQLRequest
        {
            Query = @"
			    query($beginTime:DateTime!,$endTime:DateTime!,$caAddress:String,$skipCount:Int!,$maxResultCount:Int!) {
                  getGameHistoryList(getGameHistoryDto:{beginTime:$beginTime,endTime:$endTime,caAddress:$caAddress,skipCount:$skipCount,maxResultCount:$maxResultCount}){
                    gameList{
                      id
                      caAddress
                      chainId
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