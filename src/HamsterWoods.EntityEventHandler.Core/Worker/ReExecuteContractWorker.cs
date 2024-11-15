using System.Threading.Tasks;
using HamsterWoods.EntityEventHandler.Core.Services;
using HamsterWoods.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler.Core.Worker;

public class ReExecuteContractWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IReExecuteContractService _contractService;

    public ReExecuteContractWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IReExecuteContractService contractService, IOptionsMonitor<PointJobOptions> options) : base(timer,
        serviceScopeFactory)
    {
        _contractService = contractService;
        timer.Period = options.CurrentValue.ContractReExecutePeriod * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("[ReExecuteContract] ReExecuteContractWorker start.");
        await _contractService.ReExecuteContractAsync();
        Logger.LogInformation("[ReExecuteContract] ReExecuteContractWorker end.");
    }
}