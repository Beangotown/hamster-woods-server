using System.Threading.Tasks;
using HamsterWoods.Info;
using HamsterWoods.Rank;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Info")]
[Route("api/app/info/")]
public class InfoController: HamsterWoodsBaseController
{
    private readonly IInfoAppService _infoAppService;

    public InfoController(IInfoAppService infoAppService)
    {
        _infoAppService = infoAppService;
    }

    [HttpGet]
    [Route("currentRaceInfo")]
    public async Task<CurrentRaceInfoCache> GetCurrentRaceInfoAsync()
    {
        return await _infoAppService.GetCurrentRaceInfoAsync();
    }
}