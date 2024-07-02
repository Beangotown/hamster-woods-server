using HamsterWoods.NFT;

namespace HamsterWoods.Rank;

public class RankDto
{
    public string CaAddress { get; set; }
    public long Score { get; set; }
    public int Rank { get; set; }
}

public class SettleDaySelfRank : RankDto
{
    public NftInfo RewardNftInfo { get; set; }
}

public class SettleDayRank : RankDto
{
    public int FromRank { get; set; }
    public int ToRank { get; set; }
    public int FromScore { get; set; }
    public int ToScore { get; set; }
    public NftInfo RewardNftInfo { get; set; }
}