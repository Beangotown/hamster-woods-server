using HamsterWoods.Enums;
using Orleans;

namespace HamsterWoods.Grains.Grain.Points;

public interface IPointsInfoGrain : IGrainWithStringKey
{
    Task<GrainResultDto<PointsInfoGrainDto>> Create(PointsInfoGrainDto grainDto);
    Task<GrainResultDto<PointsInfoGrainDto>> UpdateBizInfo(string bizId);
    Task<GrainResultDto<PointsInfoGrainDto>> UpdateFinalStatus(ContractInvokeStatus status);
}