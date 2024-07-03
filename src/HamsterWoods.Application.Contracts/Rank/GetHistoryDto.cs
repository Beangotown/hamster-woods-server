using HamsterWoods.NFT;

namespace HamsterWoods.Rank;

public class GetHistoryDto
{
    public string Time { get; set; }
    public string CaAddress { get; set; }
    public long Score { get; set; }
    public int Decimals { get; set; }
    public int Rank { get; set; }
    public NftInfo RewardNftInfo { get; set; }
}