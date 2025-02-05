using AElf.Client.Service;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace HamsterWoods.Grains;

[DependsOn(typeof(HamsterWoodsApplicationContractsModule),
    typeof(AbpAutoMapperModule))]
public class HamsterWoodsGrainsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<HamsterWoodsGrainsModule>(); });

        var configuration = context.Services.GetConfiguration();
        Configure<ChainOptions>(configuration.GetSection("Chains"));
        context.Services.AddSingleton<IBlockchainClientFactory<AElfClient>, AElfClientFactory>();
    }
}