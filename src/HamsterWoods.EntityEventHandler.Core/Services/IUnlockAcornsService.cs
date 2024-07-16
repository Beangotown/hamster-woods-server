using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.EntityEventHandler.Core.Services;

public interface IUnlockAcornsService
{
}

public class UnlockAcornsService : IUnlockAcornsService, ISingletonDependency
{
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<UnlockAcornsService> _logger;
    
    public UnlockAcornsService(IObjectMapper objectMapper, ILogger<UnlockAcornsService> logger)
    {
        _objectMapper = objectMapper;
        _logger = logger;
    }
}