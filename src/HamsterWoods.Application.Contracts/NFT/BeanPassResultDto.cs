namespace HamsterWoods.NFT;

public class BeanPassInfoDto
{
    public string Symbol { get; set; }
    public string TokenName { get; set; }
    public string NftImageUrl { get; set; }
}

public class BeanPassResultDto : BeanPassInfoDto
{
    public bool Owned { get; set; }
    public bool UsingBeanPass { get; set; }
}