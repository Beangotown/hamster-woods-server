using System;
using System.Threading.Tasks;
using AElf.ExceptionHandler;

namespace HamsterWoods.EntityEventHandler.Core;

public class EventHandlerExceptionHandlingService
{
    public static async Task<FlowBehavior> HandleCreateBatchSettleException(Exception e)
    {
        return new FlowBehavior
        {
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.Return,
            ReturnValue = null
        };
    }
}