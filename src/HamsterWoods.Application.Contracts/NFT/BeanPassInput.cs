namespace HamsterWoods.NFT;

public class BeanPassInput
{
    public string CaAddress { get; set; }
}

public class GetBeanPassInput : BeanPassInput
{
    public string Symbol { get; set; }
}