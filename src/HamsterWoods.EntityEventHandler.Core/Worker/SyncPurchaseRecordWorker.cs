using System.Threading.Tasks;
using HamsterWoods.EntityEventHandler.Core.Services;
using HamsterWoods.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler.Core.Worker;

public class SyncPurchaseRecordWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ISyncPurchaseRecordService _syncPurchaseRecordService;

    public SyncPurchaseRecordWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<PointJobOptions> options, ISyncPurchaseRecordService syncPurchaseRecordService) : base(timer,
        serviceScopeFactory)
    {
        _syncPurchaseRecordService = syncPurchaseRecordService;
        timer.RunOnStart = true;
        timer.Period = options.CurrentValue.SyncPurchaseRecordPeriod * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await _syncPurchaseRecordService.SyncPurchaseRecordAsync();
    }
}