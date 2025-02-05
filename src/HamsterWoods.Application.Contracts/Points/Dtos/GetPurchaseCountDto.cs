namespace HamsterWoods.Points.Dtos;

public class GetPurchaseCountDtoGraphDto
{
    public GetPurchaseCountDto GetPurchaseCount { get; set; }
}

public class GetPurchaseCountDto
{
    public long PurchaseCount { get; set; }
}

public class GetHopCountDtoGraphDto
{
    public GetHopCountDto GetHopCount { get; set; }
}

public class GetHopCountDto
{
    public long HopCount { get; set; }
}