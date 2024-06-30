using HamsterWoods.Cache;
using HamsterWoods.Common;
using HamsterWoods.Contract;
using HamsterWoods.Options;
using HamsterWoods.Grains;
using HamsterWoods.Portkey;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.DistributedLocking;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace HamsterWoods;

[DependsOn(
    typeof(HamsterWoodsDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(HamsterWoodsApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(HamsterWoodsGrainsModule),
    typeof(AbpDistributedLockingModule)
)]
public class HamsterWoodsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<HamsterWoodsApplicationModule>(); });
        context.Services.AddHttpClient();
        context.Services.AddSingleton<ICacheProvider, RedisCacheProvider>();

        var configuration = context.Services.GetConfiguration();
        Configure<ScheduledTasksOptions>(configuration.GetSection("ScheduledTasks"));
        Configure<ChainOptions>(configuration.GetSection("Chains"));
        Configure<PortkeyOptions>(configuration.GetSection("Portkey"));
    }
}