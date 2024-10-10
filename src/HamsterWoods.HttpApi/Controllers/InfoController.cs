using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using HamsterWoods.Info;
using HamsterWoods.Info.Dtos;
using HamsterWoods.Points;
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
    private readonly IPointHubService _pointHubService;

    public InfoController(IInfoAppService infoAppService, IPointHubService pointHubService)
    {
        _infoAppService = infoAppService;
        _pointHubService = pointHubService;
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

    [HttpGet]
    [Route("get-points")]
    public async Task<object> GetPointsAsync(string address, string connectionId)
    {
        return await _pointHubService.GetFluxPointsAsync(address, connectionId);
    }
}