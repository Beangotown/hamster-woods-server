namespace HamsterWoods.NFT;

public class HamsterPassDto
{
    public bool Claimable { get; set; }
    public string Reason { get; set; }
    public string TransactionId { get; set; }

    public HamsterPassInfoDto HamsterPassInfo { get; set; }
}