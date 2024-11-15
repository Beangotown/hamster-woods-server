namespace HamsterWoods.Options;

public class PointJobOptions
{
    public int SyncHopRecordPeriod { get; set; } = 5;
    public int SyncPurchaseRecordPeriod { get; set; } = 5;
    public int CreateSettlePeriod { get; set; } = 10;
    public int ContractInvokePeriod { get; set; } = 10;
    public int CreateSettleLimit { get; set; } = 1000;
    public int SettleCount { get; set; } = 1000;
    public int ContractReExecutePeriod { get; set; } = 600;
}