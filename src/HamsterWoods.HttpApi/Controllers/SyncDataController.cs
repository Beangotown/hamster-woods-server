using System.Threading.Tasks;
using Asp.Versioning;
using HamsterWoods.SyncData;
using HamsterWoods.SyncData.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("SyncData")]
[Route("api/app/sync/")]
[IgnoreAntiforgeryToken]
public class SyncDataController : HamsterWoodsBaseController
{
    private readonly ISyncDataService _syncDataService;

    public SyncDataController(ISyncDataService syncDataService)
    {
        _syncDataService = syncDataService;
    }

    [HttpPost("current-race-info")]
    public async Task<CurrentRaceDto> SyncCurrentRaceInfoAsync()
    {
        return await _syncDataService.SyncRaceConfigAsync();
    }
}