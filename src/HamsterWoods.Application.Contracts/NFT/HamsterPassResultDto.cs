namespace HamsterWoods.NFT;

public class HamsterPassInfoDto
{
    public string Symbol { get; set; }
    public string TokenName { get; set; }
    public string NftImageUrl { get; set; }
}

public class HamsterPassResultDto : HamsterPassInfoDto
{
    public bool Owned { get; set; }
    public bool UsingBeanPass { get; set; }
}