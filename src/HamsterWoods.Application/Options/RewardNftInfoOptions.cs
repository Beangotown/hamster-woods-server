namespace HamsterWoods.Options;

public class RewardNftInfoOptions
{
    public string Symbol { get; set; } = "KINGHAMSTER-1";
    public string ChainId { get; set; } = "tDVW";
    public string TokenName { get; set; } = "King of Hamsters";

    public string ImageUrl { get; set; } =
        "https://hamster-testnet.s3.ap-northeast-1.amazonaws.com/Acorns/NFT_KingHamster.png";
    public int TokenId { get; set; } = 1;
}