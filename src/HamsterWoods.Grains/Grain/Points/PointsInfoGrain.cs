using HamsterWoods.Enums;
using HamsterWoods.Grains.State.Points;
using Orleans;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Grains.Grain.Points;

public class PointsInfoGrain : Grain<PointsInfoState>, IPointsInfoGrain
{
    private readonly IObjectMapper _objectMapper;

    public PointsInfoGrain(IObjectMapper objectMapper)
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

    public async Task<GrainResultDto<PointsInfoGrainDto>> Create(PointsInfoGrainDto grainDto)
    {
        if (!State.Id.IsNullOrEmpty())
        {
            return GetResultDto();
        }

        State = _objectMapper.Map<PointsInfoGrainDto, PointsInfoState>(grainDto);
        State.Amount = grainDto.Amount;
        State.Id = this.GetPrimaryKeyString();
        State.ContractInvokeStatus = ContractInvokeStatus.None;
        State.CreateTime = DateTime.UtcNow;
        await WriteStateAsync();
        return GetResultDto();
    }

    public async Task<GrainResultDto<PointsInfoGrainDto>> UpdateBizInfo(string bizId)
    {
        if (!State.BizId.IsNullOrEmpty())
        {
            var result = GetResultDto(false, "bizId already exists.");
            result.Data = _objectMapper.Map<PointsInfoState, PointsInfoGrainDto>(State);
        }

        State.BizId = bizId;
        State.ContractInvokeStatus = ContractInvokeStatus.ToBeCreated;
        await WriteStateAsync();
        return GetResultDto();
    }

    public async Task<GrainResultDto<PointsInfoGrainDto>> UpdateFinalStatus(ContractInvokeStatus status)
    {
        if (status != ContractInvokeStatus.Success && status != ContractInvokeStatus.FinalFailed)
        {
            return GetResultDto();
        }

        State.ContractInvokeStatus = status;
        await WriteStateAsync();
        return GetResultDto();
    }

    private GrainResultDto<PointsInfoGrainDto> GetResultDto(bool success = true, string errorMessage = "")
    {
        if (success)
            return new GrainResultDto<PointsInfoGrainDto>(
                _objectMapper.Map<PointsInfoState, PointsInfoGrainDto>(State));

        var resultDto = new GrainResultDto<PointsInfoGrainDto>();
        return resultDto.Error(errorMessage);
    }
}