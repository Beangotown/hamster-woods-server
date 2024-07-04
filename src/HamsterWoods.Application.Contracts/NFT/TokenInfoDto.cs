namespace HamsterWoods.NFT;

public class TokenInfoDto
{
    public long Balance { get; set; }
    public int Decimals { get; set; }
    public decimal BalanceInUsd { get; set; }
    public string TokenContractAddress { get; set; }
}