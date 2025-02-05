using HamsterWoods.Grains.Grain.Points;

namespace HamsterWoods.Grains.State.Points;

public class ContractInvokeState: ContractInvokeGrainDto
{
    public long RefBlockNumber { get; set; }
}