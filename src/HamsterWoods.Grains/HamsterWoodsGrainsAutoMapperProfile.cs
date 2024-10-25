using AutoMapper;
using HamsterWoods.Grains.Grain.HealthCheck;
using HamsterWoods.Grains.Grain.Points;
using HamsterWoods.Grains.Grain.Unlock;
using HamsterWoods.Grains.State.HealthCheck;
using HamsterWoods.Grains.State.Points;
using HamsterWoods.Grains.State.Unlock;

namespace HamsterWoods.Grains;

public class HamsterWoodsGrainsAutoMapperProfile : Profile
{
    public HamsterWoodsGrainsAutoMapperProfile()
    {
        CreateMap<ContractInvokeState, ContractInvokeGrainDto>().ReverseMap();
        CreateMap<PointsInfoGrainDto, PointsInfoState>().ReverseMap();
        CreateMap<UnlockInfoState, UnlockInfoGrainDto>().ReverseMap();
        CreateMap<HealthCheckGrainDto, HealthCheckState>().ReverseMap();
    }
}