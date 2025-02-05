using System;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.TokenLock;
using HamsterWoods.Unlock.Etos;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.EntityEventHandler.Core.Handlers;

public class UnlockInfoHandler : IDistributedEventHandler<UnlockInfoEto>, ITransientDependency
{
    private readonly INESTRepository<UnlockInfoIndex, string> _repository;
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<UnlockInfoHandler> _logger;

    public UnlockInfoHandler(INESTRepository<UnlockInfoIndex, string> repository, IObjectMapper objectMapper,
        ILogger<UnlockInfoHandler> logger)
    {
        _repository = repository;
        _objectMapper = objectMapper;
        _logger = logger;
    }

    public async Task HandleEventAsync(UnlockInfoEto eventData)
    {
        try
        {
            await _repository.AddOrUpdateAsync(_objectMapper.Map<UnlockInfoEto, UnlockInfoIndex>(eventData));
            _logger.LogInformation("[UnlockInfoIndex] add or update success, bizId:{bizId}", eventData.BizId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UnlockInfoIndex] add or update UnlockInfoIndex error.");
        }
    }
}