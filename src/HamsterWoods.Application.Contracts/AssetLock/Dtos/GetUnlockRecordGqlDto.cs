using System;
using System.Collections.Generic;

namespace HamsterWoods.AssetLock.Dtos;

public class GetUnlockRecordGqlResultDto
{
    public GetUnlockRecordGqlDto GetUnLockedRecords { get; set; }
}

public class GetUnlockRecordGqlDto
{
    public List<GetUnlockRecordGqlItemDto> UnLockRecordList { get; set; }
}

public class GetUnlockRecordGqlItemDto
{
    public string CaAddress { get; set; }
    public string FromAddress { get; set; }
    public string Symbol { get; set; }
    public long Amount { get; set; }
    public int Decimals { get; set; }
    public int WeekNum { get; set; }
    public DateTime BlockTime { get; set; }
    public TransactionGqlInfoDto TransactionInfo { get; set; }
}

public class TransactionGqlInfoDto
{
    public string TransactionId { get; set; }
    public long TransactionFee { get; set; }
    public DateTime TriggerTime { get; set; }
}