using AutoMapper;
using HamsterWoods.EntityEventHandler.Core.Services.Dtos;
using HamsterWoods.Rank;

namespace HamsterWoods.EntityEventHandler.Core;

public class HamsterWoodsEventHandlerAutoMapperProfile : Profile
{
    public HamsterWoodsEventHandlerAutoMapperProfile()
    {
        CreateMap<RankRecordDto, UserWeekRankRecordIndex>();
    }
}