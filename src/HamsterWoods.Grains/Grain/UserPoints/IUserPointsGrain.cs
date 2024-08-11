using HamsterWoods.UserPoints.Dtos;
using Orleans;

namespace HamsterWoods.Grains.Grain.UserPoints;

public interface IUserPointsGrain: IGrainWithStringKey
{
    Task<GrainResultDto<SetHopDto>> SetHop(List<string> ids);
}