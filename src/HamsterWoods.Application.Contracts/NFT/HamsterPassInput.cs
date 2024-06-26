namespace HamsterWoods.NFT;

public class HamsterPassInput
{
    public string CaAddress { get; set; }
}

public class GetHamsterPassInput : HamsterPassInput
{
    public string Symbol { get; set; }
}