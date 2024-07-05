using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler.Core.Worker;

public class SyncPriceWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ILogger<SyncRankRecordWorker> _logger;

    public SyncPriceWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        ILogger<SyncRankRecordWorker> logger) : base(timer, serviceScopeFactory)
    {
        _logger = logger;
        Timer.Period = 1000 * 3;
        Timer.RunOnStart = true;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        _logger.LogInformation("[SyncPrice]SyncRankRecordWorker Start.");
        //await _syncRankRecordService.SyncRankRecordAsync();
        _logger.LogInformation("[SyncPrice]SyncRankRecordWorker End.");
    }
}