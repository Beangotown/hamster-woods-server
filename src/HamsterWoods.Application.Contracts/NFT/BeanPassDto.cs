namespace HamsterWoods.NFT;

public class BeanPassDto
{
    public bool Claimable { get; set; }
    public string Reason { get; set; }
    public string TransactionId { get; set; }

    public BeanPassInfoDto BeanPassInfoDto { get; set; }
}