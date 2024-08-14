using AutoMapper;
using HamsterWoods.EntityEventHandler.Core.Services.Dtos;
using HamsterWoods.Grains.Grain.Points;
using HamsterWoods.Points;
using HamsterWoods.Points.Etos;
using HamsterWoods.Rank;
using HamsterWoods.Trace;

namespace HamsterWoods.EntityEventHandler.Core;

public class HamsterWoodsEventHandlerAutoMapperProfile : Profile
{
    public HamsterWoodsEventHandlerAutoMapperProfile()
    {
        CreateMap<RankRecordDto, UserWeekRankRecordIndex>();
        CreateMap<GameResultDto, HopRecordIndex>()
            .ForMember(t => t.TransactionId,
            f => f.MapFrom(m => m.BingoTransactionInfo.TransactionId))
            .ForMember(t => t.TriggerTime,
                f => f.MapFrom(m => m.BingoTransactionInfo.TriggerTime));
        
        CreateMap<PurchaseResultDto, PurchaseRecordIndex>()
            .ForMember(t => t.TransactionId,
                f => f.MapFrom(m => m.TransactionInfo.TransactionId))
            .ForMember(t => t.TriggerTime,
                f => f.MapFrom(m => m.TransactionInfo.TriggerTime));

        CreateMap<PointsInfoGrainDto, PointsInfoIndex>()
            .ForMember(t => t.PointType, f => f.MapFrom(m => m.PointType.ToString()))
            .ForMember(t => t.ContractInvokeStatus, f => f.MapFrom(m => m.ContractInvokeStatus.ToString()));
        
        CreateMap<ContractInvokeEto, ContractInvokeIndex>();
    }
}