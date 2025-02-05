using HamsterWoods;
using HamsterWoods.MongoDb;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace HamsterWoods.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(HamsterWoodsPushMongoDbModule),
    typeof(HamsterWoodsApplicationContractsModule)
    )]
public class MessagePushDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
    }
}
