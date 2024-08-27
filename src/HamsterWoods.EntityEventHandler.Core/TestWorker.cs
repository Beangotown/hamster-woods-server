using System.Threading.Tasks;
using HamsterWoods.EntityEventHandler.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace HamsterWoods.EntityEventHandler.Core;

public class TestWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IUnlockAcornsService _unlockAcornsService;

    public TestWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IUnlockAcornsService unlockAcornsService) : base(timer, serviceScopeFactory)
    {
        _unlockAcornsService = unlockAcornsService;
        timer.RunOnStart = true;
        timer.Period = 10000 * 3000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await _unlockAcornsService.HandleAsync();
    }
}