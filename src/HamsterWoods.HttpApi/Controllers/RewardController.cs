using System.Threading.Tasks;
using Asp.Versioning;
using HamsterWoods.NFT;
using HamsterWoods.Reward;
using HamsterWoods.Reward.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Reward")]
[Route("api/app/reward/")]
[IgnoreAntiforgeryToken]
public class RewardController: HamsterWoodsBaseController
{
    private readonly IRewardAppService _rewardAppService;
    
    public RewardController(IRewardAppService rewardAppService)
    {
        _rewardAppService = rewardAppService;
    }
    
    [HttpPost]
    [Route("claim")]
    public async Task<KingHamsterClaimDto> ClaimHamsterPassAsync(HamsterPassInput input)
    {
        return await _rewardAppService.ClaimHamsterKingAsync(input);
    }
}