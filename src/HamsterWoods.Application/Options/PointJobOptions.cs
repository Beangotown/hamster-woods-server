namespace HamsterWoods.Options;

public class PointJobOptions
{
    public int SyncHopRecordPeriod { get; set; } = 500;
    public int CreateSettlePeriod { get; set; } = 500;
    public int CreateSettleLimit { get; set; } = 1000;
    public int SettleCount { get; set; } = 1000;
}