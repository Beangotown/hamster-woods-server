namespace HamsterWoods.AssetLock;

public class AssetLockedInfoDto
{
    public string LockedTime { get; set; }
    public string UnLockTime { get; set; }
    public string Symbol { get; set; }
    public int Decimals { get; set; }
    public long Amount { get; set; }
}