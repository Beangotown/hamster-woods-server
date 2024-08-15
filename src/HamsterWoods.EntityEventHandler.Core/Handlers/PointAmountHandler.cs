using System;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Points;
using HamsterWoods.Points.Etos;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.EntityEventHandler.Core.Handlers;

public class PointAmountHandler : IDistributedEventHandler<PointAmountEto>, ITransientDependency
{
    private readonly INESTRepository<PointAmountIndex, string> _repository;
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<PointAmountHandler> _logger;

    public PointAmountHandler(ILogger<PointAmountHandler> logger, IObjectMapper objectMapper,
        INESTRepository<PointAmountIndex, string> repository)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _repository = repository;
    }

    public async Task HandleEventAsync(PointAmountEto eventData)
    {
        try
        {
            await _repository.AddOrUpdateAsync(_objectMapper.Map<PointAmountEto, PointAmountIndex>(eventData));
        }
        catch (Exception e)
        {
            _logger.LogError(e,"[PointAmountIndex] add or update PointAmountIndex error.");
        }
    }
}