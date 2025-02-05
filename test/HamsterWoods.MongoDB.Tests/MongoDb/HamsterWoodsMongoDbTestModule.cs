using Volo.Abp.Modularity;

namespace HamsterWoods.MongoDb;

[DependsOn(
    typeof(HamsterWoodsTestBaseModule),
    typeof(HamsterWoodsPushMongoDbModule)
    )]
public class HamsterWoodsMongoDbTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // var stringArray = IMMongoDbFixture.ConnectionString.Split('?');
        // var connectionString = stringArray[0].EnsureEndsWith('/') +
        //                            "Db_" +
        //                        Guid.NewGuid().ToString("N") + "/?" + stringArray[1];
        //
        // Configure<AbpDbConnectionOptions>(options =>
        // {
        //     options.ConnectionStrings.Default = connectionString;
        // });
    }
}
