using System;
using System.Threading.Tasks;
using AElf.ExceptionHandler;
using HamsterWoods.Commons;
using HamsterWoods.NFT;
using Volo.Abp;

namespace HamsterWoods;

public class ExceptionHandlingService
{
    public static async Task<FlowBehavior> HandleJoinException(Exception e)
    {
        return new FlowBehavior
        {
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.Return,
            ReturnValue = false
        };
    }
    
    public static async Task<FlowBehavior> HandleGetCaHolderCreateTimeException(Exception e)
    {
        return new FlowBehavior
        {
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.Return,
            ReturnValue = new UserFriendlyException(HamsterWoodsConstants.SyncingMessage, HamsterWoodsConstants.SyncingCode)
        };
    }
    
    public static async Task<FlowBehavior> HandleGetHolderTokenInfoException(Exception e)
    {
        return new FlowBehavior
        {
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.Return,
            ReturnValue = new TokenInfoDto()
        };
    }
}