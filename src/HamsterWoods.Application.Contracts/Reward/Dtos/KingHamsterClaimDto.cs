using HamsterWoods.NFT;

namespace HamsterWoods.Reward.Dtos;

public class KingHamsterClaimDto
{
    public bool Claimable { get; set; }
    public string Reason { get; set; }
    public string TransactionId { get; set; }

    public HamsterPassInfoDto KingHamsterInfo { get; set; }
}

public class HamsterRewardNftInfo : HamsterPassInfoDto
{
    public int Amount { get; set; }
}