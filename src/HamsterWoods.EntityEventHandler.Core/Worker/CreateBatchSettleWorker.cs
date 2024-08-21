using System.Threading.Tasks;
using HamsterWoods.EntityEventHandler.Core.Services;
using HamsterWoods.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler.Core.Worker;

public class CreateBatchSettleWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ICreateBatchSettleService _createBatchSettleService;

    public CreateBatchSettleWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<PointJobOptions> options, ICreateBatchSettleService createBatchSettleService) : base(
        timer, serviceScopeFactory)
    {
        _createBatchSettleService = createBatchSettleService;
        timer.RunOnStart = false;
        timer.Period = options.CurrentValue.CreateSettlePeriod * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await _createBatchSettleService.CreateBatchSettleAsync();
    }
}