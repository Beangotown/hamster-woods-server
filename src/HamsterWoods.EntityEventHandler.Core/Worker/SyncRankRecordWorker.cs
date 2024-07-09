using System.Threading.Tasks;
using HamsterWoods.EntityEventHandler.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler.Core.Worker;

public class SyncRankRecordWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ISyncRankRecordService _syncRankRecordService;
    private readonly ILogger<SyncRankRecordWorker> _logger;

    public SyncRankRecordWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        ISyncRankRecordService syncRankRecordService,
        ILogger<SyncRankRecordWorker> logger) : base(timer,
        serviceScopeFactory)
    {
        _syncRankRecordService = syncRankRecordService;
        _logger = logger;
        Timer.RunOnStart = true;
        Timer.Period = 1000 * 60 * 60 * 24;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        _logger.LogInformation("[SyncRankRecord]SyncRankRecordWorker Start.");
        //await _syncRankRecordService.SyncRankRecordAsync();
        _logger.LogInformation("[SyncRankRecord]SyncRankRecordWorker End.");
    }
}