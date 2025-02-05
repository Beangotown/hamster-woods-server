using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.EntityEventHandler.Core;

public class EntityHandlerBase : ITransientDependency
{
    public IAbpLazyServiceProvider LazyServiceProvider { get; set; }
    
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();
    
}