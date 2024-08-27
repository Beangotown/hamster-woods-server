using HamsterWoods.Options;
using Hangfire;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.EntityEventHandler.Core.Services;

public interface IInitJobsService
{
    void InitRecurringJob();
}

public class InitJobsService : IInitJobsService, ISingletonDependency
{
    private readonly IRecurringJobManager _recurringJobs;
    private readonly BackgroundJobOptions _options;

    public InitJobsService(IRecurringJobManager recurringJobs, IOptionsMonitor<BackgroundJobOptions> options)
    {
        _recurringJobs = recurringJobs;
        _options = options.CurrentValue;
    }
    
    public void InitRecurringJob()
    {
        _recurringJobs.AddOrUpdate<ISyncRankRecordService>("SyncRankRecordService",
            x => x.SyncRankRecordAsync(), _options.SyncRecordCorn);
        
        // _recurringJobs.AddOrUpdate<IUnlockAcornsService>("UnlockAcornsService",
        //     x => x.HandleAsync(), "*/5 * * * * ?");
    }
}