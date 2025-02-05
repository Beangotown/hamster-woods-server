namespace HamsterWoods.Grains.Grain.Options;

public class PointsOptions
{
    public int MaxRetryCount { get; set; } = 5;

    public int BlockPackingMaxHeightDiff { get; set; } = 512;
}