using AutoMapper;
using HamsterWoods.Cache;
using HamsterWoods.Grains.Grain.Points;
using HamsterWoods.NFT;
using HamsterWoods.Options;
using HamsterWoods.Points;
using HamsterWoods.Points.Dtos;
using HamsterWoods.Points.Etos;
using HamsterWoods.Rank;
using HamsterWoods.SyncData.Dtos;
using HamsterWoods.TokenLock;
using HamsterWoods.Trace;

namespace HamsterWoods;

public class HamsterWoodsApplicationAutoMapperProfile : Profile
{
    public HamsterWoodsApplicationAutoMapperProfile()
    {
        CreateMap<RankDto, UserWeekRankIndex>().ForMember(dest => dest.SumScore,
            opts => opts.MapFrom(src => src.Score)).ReverseMap();
        CreateMap<UserWeekRankIndex, WeekRankDto>().ForMember(destination => destination.Score,
            opt => opt.MapFrom(source => source.SumScore));
        CreateMap<WeekRankDto, UserWeekRankIndex>().ForMember(dest => dest.SumScore,
            opts => opts.MapFrom(src => src.Score)).ReverseMap();
        CreateMap<GetUserActionDto, UserActionIndex>();
        CreateMap<HamsterPassInfoDto, HamsterPassResultDto>().ReverseMap();
        CreateMap<RewardNftInfoOptions, NftInfo>();
        CreateMap<UserWeekRankRecordIndex, UserWeekRankDto>();
        CreateMap<RaceInfoConfigIndex, CurrentRaceDto>();
        CreateMap<PriceInfo, PriceDto>()
            .ForMember(t => t.ElfInUsd, opt => opt.MapFrom(f => f.ElfToUsd))
            .ForMember(t => t.AcornsInElf, opt => opt.MapFrom(f => f.AcornsToElf))
            .ForMember(t => t.AcornsInUsd, opt => opt.MapFrom(f => f.AcornsToUsd));


        CreateMap<ContractInvokeIndex, ContractInvokeEto>();
        CreateMap<ContractInvokeEto, ContractInvokeGrainDto>().ReverseMap();
        CreateMap<HopConfig, DailyDto>();
        CreateMap<ChanceConfig, WeeklyDto>();
    }
}