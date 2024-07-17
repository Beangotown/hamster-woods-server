using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace HamsterWoods.Monitor;

public class HamsterWoodsServerMonitorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<IndicatorOptions>(context.Services.GetConfiguration().GetSection("Indicator"));
    }
}