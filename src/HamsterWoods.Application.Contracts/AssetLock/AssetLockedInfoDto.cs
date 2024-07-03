using System.Collections.Generic;

namespace HamsterWoods.AssetLock;

public class AssetLockedInfoResultDto
{
    public long TotalLockedAmount { get; set; }
    public int Decimals { get; set; }
    public List<AssetLockedInfoDto> LockedInfoList { get; set; }
}
public class AssetLockedInfoDto
{
    public string LockedTime { get; set; }
    public string UnLockTime { get; set; }
    public string Symbol { get; set; }
    public int Decimals { get; set; }
    public long Amount { get; set; }
}