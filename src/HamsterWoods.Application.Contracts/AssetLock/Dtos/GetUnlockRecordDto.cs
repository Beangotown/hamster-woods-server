namespace HamsterWoods.AssetLock.Dtos;

public class GetUnlockRecordDto
{
    public string UnLockTime { get; set; }
    public string Symbol { get; set; }
    public int Decimals { get; set; }
    public long Amount { get; set; }
    public string TransactionId { get; set; }
}