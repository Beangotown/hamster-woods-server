using Orleans.TestingHost;

namespace HamsterWoods.Grain.Tests;

public class HamsterWoodsGrainTestBase :HamsterWoodsTestBase<HamsterWoodsGrainTestModule>
{
    protected readonly TestCluster Cluster;

    public HamsterWoodsGrainTestBase()
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;

    }
}