using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.Points;
using HamsterWoods.Points.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Points")]
[Route("api/app/points/")]
public class PointsController : HamsterWoodsBaseController
{
    private readonly IPointsAppService _pointsAppService;

    public PointsController(IPointsAppService pointsAppService)
    {
        _pointsAppService = pointsAppService;
    }

    [HttpGet("daily")]
    public async Task<List<DailyDto>> GetDailyListAsync(string caAddress) =>
        await _pointsAppService.GetDailyListAsync(caAddress);
    
    [HttpGet("weekly")]
    public async Task<List<WeeklyDto>> GetWeeklyListAsync(string caAddress) =>
        await _pointsAppService.GetWeeklyListAsync(caAddress);
}