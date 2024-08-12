using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Commons;
using HamsterWoods.Enums;
using HamsterWoods.Options;
using HamsterWoods.Points;
using HamsterWoods.Points.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
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

    public CreateBatchSettleService(ILogger<CreateBatchSettleService> logger, IPointSettleService pointSettleService,
        INESTRepository<PointsInfoIndex, string> pointsInfoRepository, IOptionsMonitor<PointJobOptions> options)
    {
        _logger = logger;
        _pointSettleService = pointSettleService;
        _pointsInfoRepository = pointsInfoRepository;
        _options = options.CurrentValue;
    }

    public async Task CreateBatchSettleAsync()
    {
        var list = await GetListAsync(ContractInvokeStatus.None.ToString(), CommonConstant.DefaultSkipCount,
            _options.CreateSettleLimit);
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

    private async Task<List<PointsInfoIndex>> GetListAsync(string contractInvokeStatus, int skipCount,
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