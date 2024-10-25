using Orleans;

namespace HamsterWoods.Grains.Grain.HealthCheck;

public interface IHealthCheckGrain : IGrainWithStringKey
{
    Task<GrainResultDto<HealthCheckGrainDto>> CreateOrUpdateHealthCheckData(HealthCheckGrainDto request);
    
    Task<GrainResultDto<HealthCheckGrainDto>> GetHealthCheckData();
}