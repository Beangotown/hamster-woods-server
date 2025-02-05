using System;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Commons;
using HamsterWoods.EntityEventHandler.Core.Services;
using HamsterWoods.Enums;
using HamsterWoods.Points;
using HamsterWoods.Points.Etos;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.EntityEventHandler.Core.Handlers;

public class ContractInvokeHandler : IDistributedEventHandler<ContractInvokeEto>, ITransientDependency
{
    private readonly INESTRepository<ContractInvokeIndex, string> _repository;
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<ContractInvokeHandler> _logger;
    private readonly IContractInvokeService _contractInvokeService;

    public ContractInvokeHandler(INESTRepository<ContractInvokeIndex, string> repository,
        IObjectMapper objectMapper,
        ILogger<ContractInvokeHandler> logger, IContractInvokeService contractInvokeService)
    {
        _repository = repository;
        _objectMapper = objectMapper;
        _logger = logger;
        _contractInvokeService = contractInvokeService;
    }

    public async Task HandleEventAsync(ContractInvokeEto eventData)
    {
        try
        {
            var contact = _objectMapper.Map<ContractInvokeEto, ContractInvokeIndex>(eventData);
            await _repository.AddOrUpdateAsync(contact);
            _logger.LogInformation("HandleEventAsync ContractInvokeEto success, id:{id},bizId:{bizId}", eventData.Id,
                eventData.BizId);

            // unlock acorns does not need to be sent immediately
            if (eventData.Status == ContractInvokeStatus.ToBeCreated.ToString() &&
                eventData.BizType != CommonConstant.BatchUnlockAcorns)
            {
                BackgroundJob.Enqueue(() =>
                    _contractInvokeService.ExecuteJobAsync(eventData.BizId));

                _logger.LogInformation("Enqueue ContractInvokeEto success, ,bizId:{bizId}", eventData.BizId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", JsonConvert.SerializeObject(eventData));
        }
    }
}