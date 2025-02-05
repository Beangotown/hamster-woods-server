using System.Threading.Tasks;
using HamsterWoods.EntityEventHandler.Core.Services;
using HamsterWoods.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler.Core.Worker;

public class SyncHopRecordWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ISyncHopRecordService _syncHopRecordService;

    public SyncHopRecordWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<PointJobOptions> options, ISyncHopRecordService syncHopRecordService) : base(timer,
        serviceScopeFactory)
    {
        _syncHopRecordService = syncHopRecordService;
        timer.RunOnStart = true;
        timer.Period = options.CurrentValue.SyncHopRecordPeriod * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await _syncHopRecordService.SyncHopRecordAsync();
    }
}