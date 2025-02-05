using Orleans.TestingHost;
using Volo.Abp.Modularity;

namespace HamsterWoods.Orleans.TestBase;

public abstract class HamsterWoodsOrleansTestBase<TStartupModule> : HamsterWoodsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected readonly TestCluster Cluster;

    public HamsterWoodsOrleansTestBase()
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;
    }
}