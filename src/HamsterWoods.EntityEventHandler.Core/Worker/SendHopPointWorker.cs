using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Enums;
using HamsterWoods.Points;
using HamsterWoods.Points.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler.Core.Worker;

public class SendHopPointWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly INESTRepository<PointsInfoIndex, string> _pointsInfoRepository;
    private readonly IPointSettleService _pointSettleService;

    public SendHopPointWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        INESTRepository<PointsInfoIndex, string> pointsInfoRepository, IPointSettleService pointSettleService) : base(
        timer, serviceScopeFactory)
    {
        _pointsInfoRepository = pointsInfoRepository;
        _pointSettleService = pointSettleService;
        //timer.RunOnStart = true;
        timer.Period = 50 * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var list = await GetListAsync("None");
        foreach (var item in list)
        {
            var bizId = Guid.NewGuid().ToString();
            await BatchSettleAsync(bizId, new List<PointsInfoIndex>() { item });
            item.BizId = bizId;
            item.ContractInvokeStatus = ContractInvokeStatus.ToBeCreated.ToString();
            await _pointsInfoRepository.AddOrUpdateAsync(item);
        }
    }

    private async Task BatchSettleAsync(string bizId, List<PointsInfoIndex> records)
    {
        var pointSettleDto = new PointSettleDto()
        {
            ChainId = "tDVW",
            BizId = bizId,
            PointName = "ACORNS point-2"
        };

        var points = records.Select(record => new UserPointInfo()
            { Address = record.Address, PointAmount = record.Amount }).ToList();

        pointSettleDto.UserPointsInfos = points;
        await _pointSettleService.BatchSettleAsync(pointSettleDto);
        //_logger.LogInformation("BatchSettle finish, bizId:{bizId}", bizId);
    }

    private async Task<List<PointsInfoIndex>> GetListAsync(string contractInvokeStatus)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<PointsInfoIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ContractInvokeStatus).Value(contractInvokeStatus)));

        QueryContainer Filter(QueryContainerDescriptor<PointsInfoIndex> f)
        {
            return f.Bool(b => b.Must(mustQuery));
        }

        var result = await _pointsInfoRepository.GetListAsync(Filter);

        return result.Item2;
    }
}