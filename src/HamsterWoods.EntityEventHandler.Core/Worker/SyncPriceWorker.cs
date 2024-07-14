using System.Threading.Tasks;
using HamsterWoods.EntityEventHandler.Core.Services;
using HamsterWoods.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler.Core.Worker;

public class SyncPriceWorker  : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ILogger<SyncPriceWorker> _logger;
    private readonly ISyncPriceService _syncPriceService;

    public SyncPriceWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        ILogger<SyncPriceWorker> logger, IOptionsMonitor<SyncPriceDataOptions> options,
        ISyncPriceService syncPriceService) : base(timer,
        serviceScopeFactory)
    {
        _logger = logger;
        _syncPriceService = syncPriceService;
        Timer.Period = 1000 * options.CurrentValue.Period;
        Timer.RunOnStart = true;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        _logger.LogInformation("[SyncPrice] SyncRankRecordWorker Start.");
        await _syncPriceService.SyncPriceAsync();
        _logger.LogInformation("[SyncPrice] SyncRankRecordWorker End.");
    }
}