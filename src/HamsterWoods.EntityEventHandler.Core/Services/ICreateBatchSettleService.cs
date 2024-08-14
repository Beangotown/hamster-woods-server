using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.Enums;
using HamsterWoods.Grains.Grain.Points;
using HamsterWoods.Options;
using HamsterWoods.Points;
using HamsterWoods.Points.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Orleans;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.EntityEventHandler.Core.Services;

public interface ICreateBatchSettleService
{
    Task CreateBatchSettleAsync();
}

public class CreateBatchSettleService : ICreateBatchSettleService, ISingletonDependency
{
    private readonly INESTRepository<PointsInfoIndex, string> _pointsInfoRepository;
    private readonly IPointSettleService _pointSettleService;
    private readonly ILogger<CreateBatchSettleService> _logger;
    private readonly PointJobOptions _options;
    private readonly ChainOptions _chainOptions;
    private readonly IClusterClient _clusterClient;
    private readonly IOptionsMonitor<PointTradeOptions> _pointTradeOptions;

    public CreateBatchSettleService(ILogger<CreateBatchSettleService> logger, IPointSettleService pointSettleService,
        INESTRepository<PointsInfoIndex, string> pointsInfoRepository, IOptionsMonitor<PointJobOptions> options,
        IOptionsMonitor<ChainOptions> chainOptions, IClusterClient clusterClient,
        IOptionsMonitor<PointTradeOptions> pointTradeOptions)
    {
        _logger = logger;
        _pointSettleService = pointSettleService;
        _pointsInfoRepository = pointsInfoRepository;
        _clusterClient = clusterClient;
        _options = options.CurrentValue;
        _chainOptions = chainOptions.CurrentValue;
        _pointTradeOptions = pointTradeOptions;
    }

    public async Task CreateBatchSettleAsync()
    {
        _logger.LogInformation("[CreateBatchSettle] begin CreateBatchSettle.");
        var pointsInfoList = await GetPointsInfoListAsync(ContractInvokeStatus.None.ToString(),
            CommonConstant.DefaultSkipCount,
            _options.CreateSettleLimit);

        if (pointsInfoList.IsNullOrEmpty()) return;

        var skipCount = 0;
        var settleCount = _pointTradeOptions.CurrentValue.MaxBatchSize;
        var list = pointsInfoList.Skip(skipCount).Take(settleCount).ToList();

        while (!list.IsNullOrEmpty())
        {
            await BatchSettleAsync(list);
            skipCount += settleCount;
            list = pointsInfoList.Skip(skipCount).Take(settleCount).ToList();
        }

        _logger.LogInformation("[CreateBatchSettle] end CreateBatchSettle.");
    }

    private async Task BatchSettleAsync(List<PointsInfoIndex> pointsInfoList)
    {
        var bizId = Guid.NewGuid().ToString();
        var userPointInfos = new List<UserPointInfo>();
        foreach (var item in pointsInfoList)
        {
            var pointsInfoGrain = _clusterClient.GetGrain<IPointsInfoGrain>(item.Id);
            var resultDto = await pointsInfoGrain.UpdateBizInfo(bizId);
            item.BizId = resultDto.Data.BizId;
            item.ContractInvokeStatus = resultDto.Data.ContractInvokeStatus.ToString();

            if (!resultDto.Success)
            {
                continue;
            }

            userPointInfos.Add(new UserPointInfo()
            {
                Address = item.Address,
                PointAmount = item.Amount
            });
        }

        await _pointsInfoRepository.BulkAddOrUpdateAsync(pointsInfoList);
        if (userPointInfos.IsNullOrEmpty()) return;
        await BatchSettleAsync(bizId, userPointInfos);
    }

    private async Task BatchSettleAsync(string bizId, List<UserPointInfo> userPointInfos)
    {
        var pointSettleDto = new PointSettleDto
        {
            ChainId = _chainOptions.ChainInfos.Keys.First(),
            BizId = bizId,
            PointName = "ACORNS point-2"
        };

        pointSettleDto.UserPointsInfos = userPointInfos;
        await _pointSettleService.BatchSettleAsync(pointSettleDto);
        _logger.LogInformation("[CreateBatchSettle] finish, bizId:{bizId}", bizId);
    }

    private async Task<List<PointsInfoIndex>> GetPointsInfoListAsync(string contractInvokeStatus, int skipCount,
        int maxResultCount)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<PointsInfoIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ContractInvokeStatus).Value(contractInvokeStatus)));

        QueryContainer Filter(QueryContainerDescriptor<PointsInfoIndex> f)
        {
            return f.Bool(b => b.Must(mustQuery));
        }

        var result = await _pointsInfoRepository.GetListAsync(Filter, skip: skipCount, limit: maxResultCount);
        return result.Item2;
    }
}