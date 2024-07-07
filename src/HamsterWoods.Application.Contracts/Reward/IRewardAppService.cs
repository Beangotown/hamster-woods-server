using System.Threading.Tasks;
using HamsterWoods.NFT;
using HamsterWoods.Reward.Dtos;

namespace HamsterWoods.Reward;

public interface IRewardAppService
{
    Task<KingHamsterClaimDto> ClaimHamsterKingAsync(HamsterPassInput input);
    Task<KingHamsterClaimDto> SendAsync(HamsterPassInput input);
}