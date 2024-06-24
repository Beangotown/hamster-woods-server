using HamsterWoods.Grain.Tests;
using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace HamsterWoods;

[DependsOn(
    typeof(HamsterWoodsApplicationModule),
    typeof(AbpEventBusModule),
    typeof(HamsterWoodsGrainTestModule),
    typeof(HamsterWoodsDomainTestModule)
)]
public class HamsterWoodsApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // context.Services.AddSingleton(sp => sp.GetService<ClusterFixture>().Cluster.Client);
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<HamsterWoodsApplicationModule>(); });
        
        base.ConfigureServices(context);
    }
}