using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using HamsterWoods.Rank;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Rank")]
[Route("api/app/rank/")]
public class RankController: HamsterWoodsBaseController
{
    private readonly IRankService _rankService;

    public RankController(IRankService rankService)
    {
        _rankService = rankService;
    }

    [HttpGet]
    [Route("week-rank")]
    public async Task<WeekRankResultDto> GetWeekRankAsync(GetRankDto input)
    {
        return await _rankService.GetWeekRankAsync(input);
    }

    [HttpGet]
    [Route("history")]
    public async Task<List<GetHistoryDto>> GetHistoryAsync(GetRankDto input)
    {
        return await _rankService.GetHistoryAsync(input);
    }
}