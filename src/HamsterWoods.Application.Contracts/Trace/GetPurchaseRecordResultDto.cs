using System.Collections.Generic;

namespace HamsterWoods.Trace;

public class GetPurchaseRecordResultDto
{
    public GetPurchaseRecordDto GetPurchaseRecords { get; set; }
}

public class GetPurchaseRecordDto
{
    public List<PurchaseResultDto> BuyChanceList { get; set; }
}

public class PurchaseResultDto
{
    public string Id { get; set; }
    public string CaAddress { get; set; }
    public int Chance { get; set; }
    public long Cost { get; set; }
    public string Symbol { get; set; }
    public int Decimals { get; set; }
    public long TranscationFee { get; set; }
    public TransactionInfoDto? TransactionInfo { get; set; }
}