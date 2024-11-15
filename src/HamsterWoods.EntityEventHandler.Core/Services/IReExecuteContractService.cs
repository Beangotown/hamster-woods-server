using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Enums;
using HamsterWoods.Grains.Grain.Points;
using HamsterWoods.Points;
using HamsterWoods.Points.Etos;
using Microsoft.Extensions.Logging;
using Nest;
using Orleans;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.EntityEventHandler.Core.Services;

public interface IReExecuteContractService
{
    Task ReExecuteContractAsync();
}

public class ReExecuteContractService : IReExecuteContractService, ISingletonDependency
{
    private readonly INESTRepository<ContractInvokeIndex, string> _contractInvokeIndexRepository;
    private readonly ILogger<ReExecuteContractService> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly IObjectMapper _objectMapper;
    private readonly IDistributedEventBus _distributedEventBus;

    public ReExecuteContractService(INESTRepository<ContractInvokeIndex, string> contractInvokeIndexRepository,
        ILogger<ReExecuteContractService> logger, IClusterClient clusterClient, IObjectMapper objectMapper,
        IDistributedEventBus distributedEventBus)
    {
        _contractInvokeIndexRepository = contractInvokeIndexRepository;
        _logger = logger;
        _clusterClient = clusterClient;
        _objectMapper = objectMapper;
        _distributedEventBus = distributedEventBus;
    }

    public async Task ReExecuteContractAsync()
    {
        try
        {
            var records = await GetFailedTransactionAsync(100, "BatchUnlockAcorns");
            _logger.LogInformation("[ReExecuteContract] FinalFailed transaction count:{count}", records.Count);
            if (records.IsNullOrEmpty()) return;

            var bizIds = records.Where(t => t.Message.Contains("Insufficient balance of ACORNS")).Select(t => t.BizId)
                .ToList();
            _logger.LogInformation("[ReExecuteContract] need resend transaction count:{count}", bizIds.Count);

            foreach (var bizId in bizIds)
            {
                var contractInvokeGrain = _clusterClient.GetGrain<IContractInvokeGrain>(bizId);
                var result = await contractInvokeGrain.ReExecuteAsync();
                if (!result.Success)
                {
                    _logger.LogError("[ReExecuteContract] ReExecuteAsync in grain error, message:{message}",
                        result.Message);
                }

                var syncTxEtoData = _objectMapper.Map<ContractInvokeGrainDto, ContractInvokeEto>(result.Data);
                await _distributedEventBus.PublishAsync(syncTxEtoData, false, false);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[ReExecuteContract] Error, message:{message}, stackTrace:{stackTrace}", e.Message,
                e.StackTrace ?? "-");
        }
    }

    private async Task<List<ContractInvokeIndex>> GetFailedTransactionAsync(int limit, string methodName)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<ContractInvokeIndex>, QueryContainer>>
        {
            q => q.Term(i => i.Field(f => f.Status).Value(ContractInvokeStatus.FinalFailed.ToString())),
            q => q.Term(i => i.Field(f => f.ContractMethod).Value(methodName))
        };

        QueryContainer Filter(QueryContainerDescriptor<ContractInvokeIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var (totalCount, txs) = await _contractInvokeIndexRepository.GetListAsync(Filter, skip: 0, limit: limit);
        return txs ?? new List<ContractInvokeIndex>();
    }
}