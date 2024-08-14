using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.EntityEventHandler.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler.Core.Worker;

public class ContractInvokeWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IContractInvokeService _contractInvokeService;


    public ContractInvokeWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IContractInvokeService contractInvokeService) : base(timer, serviceScopeFactory)
    {
        _contractInvokeService = contractInvokeService;
        Timer.Period = 10 * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("Executing contract invoke job");
        var bizIds = await _contractInvokeService.SearchUnfinishedTransactionsAsync(10);
        var tasks = new List<Task>();
        foreach (var bizId in bizIds)
        {
            await _contractInvokeService.ExecuteJobAsync(bizId);
            //tasks.Add(Task.Run(() => { _contractInvokeService.ExecuteJobAsync(bizId); }));
        }

        //await Task.WhenAll(tasks);
    }
}