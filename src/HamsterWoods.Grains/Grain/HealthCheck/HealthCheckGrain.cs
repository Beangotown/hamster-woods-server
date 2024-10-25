using HamsterWoods.Grains.State.HealthCheck;
using Orleans;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Grains.Grain.HealthCheck;

public class HealthCheckGrain : Grain<HealthCheckState>, IHealthCheckGrain
{
    private readonly IObjectMapper _objectMapper;

    public HealthCheckGrain(IObjectMapper objectMapper)
    {
        _objectMapper = objectMapper;
    }
    
    public override async Task OnActivateAsync()
    {
        await ReadStateAsync();
        await base.OnActivateAsync();
    }

    public override async Task OnDeactivateAsync()
    {
        await WriteStateAsync();
        await base.OnDeactivateAsync();
    }
    
    public async Task<GrainResultDto<HealthCheckGrainDto>> CreateOrUpdateHealthCheckData(HealthCheckGrainDto request)
    {
        if (State.Initialized && !State.Id.IsNullOrEmpty())
        {
            State.Timestamp = request.Timestamp;
            await WriteStateAsync();
            return GetResultDto();
        }

        State.Id = request.Id;
        State.Initialized = true;
        State.Timestamp = request.Timestamp;
        await WriteStateAsync();
        return GetResultDto();
    }

    public Task<GrainResultDto<HealthCheckGrainDto>> GetHealthCheckData()
    {
        return Task.FromResult(GetResultDto());
    }
    
    private GrainResultDto<HealthCheckGrainDto> GetResultDto(bool success = true, string errorMessage = "")
    {
        if (success)
            return new GrainResultDto<HealthCheckGrainDto>(
                _objectMapper.Map<HealthCheckState, HealthCheckGrainDto>(State));

        var resultDto = new GrainResultDto<HealthCheckGrainDto>();
        return resultDto.Error(errorMessage);
    }
}