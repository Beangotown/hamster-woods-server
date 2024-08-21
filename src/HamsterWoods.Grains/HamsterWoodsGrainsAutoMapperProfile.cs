using AutoMapper;
using HamsterWoods.Grains.Grain.Points;
using HamsterWoods.Grains.State.Points;

namespace HamsterWoods.Grains;

public class HamsterWoodsGrainsAutoMapperProfile : Profile
{
    public HamsterWoodsGrainsAutoMapperProfile()
    {
        CreateMap<ContractInvokeState, ContractInvokeGrainDto>().ReverseMap();
        CreateMap<PointsInfoGrainDto, PointsInfoState>().ReverseMap();
    }
}