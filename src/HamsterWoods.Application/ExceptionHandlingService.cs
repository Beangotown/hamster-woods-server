using System;
using System.Threading.Tasks;
using AElf.ExceptionHandler;

namespace HamsterWoods;

public class ExceptionHandlingService
{
    public static async Task<FlowBehavior> HandleJoinException(Exception ex)
    {
        return new FlowBehavior
        {
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.Return,
            ReturnValue = false
        };
    }
}