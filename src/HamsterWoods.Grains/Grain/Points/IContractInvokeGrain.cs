using Orleans;

namespace HamsterWoods.Grains.Grain.Points;

public interface IContractInvokeGrain: IGrainWithStringKey
{
    Task<GrainResultDto<ContractInvokeGrainDto>> CreateAsync(ContractInvokeGrainDto input);

    Task<GrainResultDto<ContractInvokeGrainDto>> ExecuteJobAsync(ContractInvokeGrainDto input);
    Task<GrainResultDto<ContractInvokeGrainDto>> ResetUnlock();
}