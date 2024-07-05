using System.Threading.Tasks;
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
        // return new KingHamsterClaimDto
        // {
        //     Claimable=true,
        //     TransactionId="685fa94f58d5176438b678ebf317fc23fb6539adc66127c6221b7a18a4a20364",
        //     KingHamsterInfo = new HamsterPassInfoDto()
        //     {
        //         Symbol = "KingHamster-1",
        //         TokenName = "KingHamster",
        //         TokenId = 1,
        //         NftImageUrl = "https://forest-testnet.s3.ap-northeast-1.amazonaws.com/1008xAUTO/1718204222065-Activity%20Icon.png"
        //     }
        // };
    }
}