using HamsterWoods.Enums;

namespace HamsterWoods.Grains.State.Points;

public class PointsInfoState
{
    public string Id { get; set; }
    public string Address { get; set; }
    public long Amount { get; set; }
    public PointType PointType { get; set; }
    public DateTime CreateTime { get; set; }
    public string BizId { get; set; }
    public ContractInvokeStatus ContractInvokeStatus { get; set; }
}