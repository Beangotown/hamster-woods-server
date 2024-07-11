using System.Threading.Tasks;
using HamsterWoods.SyncData;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("SyncData")]
[Route("api/app/sync/")]
[IgnoreAntiforgeryToken]
public class SyncDataController: HamsterWoodsBaseController
{
    private readonly ISyncDataService _syncDataService;

    public SyncDataController(ISyncDataService syncDataService)
    {
        _syncDataService = syncDataService;
    }

    [HttpPost("current-race-info")]
    public async Task SyncCurrentRaceInfoAsync()
    {
        await _syncDataService.SyncRaceConfigAsync();
    }
}