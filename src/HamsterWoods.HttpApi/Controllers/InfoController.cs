using System.Threading.Tasks;
using HamsterWoods.Info;
using HamsterWoods.Info.Dtos;
using HamsterWoods.Rank;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Info")]
[Route("api/app/info/")]
public class InfoController : HamsterWoodsBaseController
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

    [HttpGet]
    [Route("get-val")]
    public async Task<object> GetValAsync(string key)
    {
        return await _infoAppService.GetValAsync(key);
    }

    [HttpGet("search/{indexName}")]
    public async Task<string> GetDataAsync(GetIndexDataDto input, string indexName)
    {
        return await _infoAppService.GetDataAsync(input, indexName);
    }
}