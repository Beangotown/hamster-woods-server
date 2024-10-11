using HamsterWoods.Grains.State.Unlock;
using Orleans;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Grains.Grain.Unlock;

public interface IUnlockInfoGrain : IGrainWithStringKey
{
    Task<GrainResultDto<UnlockInfoGrainDto>> SetUnlockInfo(UnlockInfoGrainDto grainDto);
}

public class UnlockInfoGrain : Grain<UnlockInfoState>, IUnlockInfoGrain
{
    private readonly IObjectMapper _objectMapper;

    public UnlockInfoGrain(IObjectMapper objectMapper)
    {
        _objectMapper = objectMapper;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await ReadStateAsync();
        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken token)
    {
        await WriteStateAsync();
        await base.OnDeactivateAsync(reason, token);
    }

    public async Task<GrainResultDto<UnlockInfoGrainDto>> SetUnlockInfo(UnlockInfoGrainDto grainDto)
    {
        var resultDto = new GrainResultDto<UnlockInfoGrainDto>();
        if (!grainDto.Id.IsNullOrEmpty())
        {
            return resultDto.Error("unlock info list is empty.");
        }

        State.Id = this.GetPrimaryKeyString();
        State.WeekNum = grainDto.WeekNum;
        State.BizId = grainDto.BizId;
        State.Addresses = grainDto.Addresses;
        State.CreateTime = DateTime.UtcNow;

        await WriteStateAsync();
        return new GrainResultDto<UnlockInfoGrainDto>(
            _objectMapper.Map<UnlockInfoState, UnlockInfoGrainDto>(State));
    }
}