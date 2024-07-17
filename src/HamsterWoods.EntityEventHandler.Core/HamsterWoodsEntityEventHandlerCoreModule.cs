using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace HamsterWoods.EntityEventHandler.Core
{
    [DependsOn(typeof(AbpAutoMapperModule), typeof(HamsterWoodsApplicationModule),
        typeof(HamsterWoodsApplicationContractsModule))]
    public class HamsterWoodsEntityEventHandlerCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                //Add all mappings defined in the assembly of the MyModule class
                options.AddMaps<HamsterWoodsEntityEventHandlerCoreModule>();
            });
        }
    }
}