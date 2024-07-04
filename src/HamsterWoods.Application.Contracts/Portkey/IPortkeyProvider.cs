using System.Threading.Tasks;
using HamsterWoods.NFT;

namespace HamsterWoods.Portkey;

public interface IPortkeyProvider
{
    public Task<long> GetCaHolderCreateTimeAsync(HamsterPassInput hamsterPassInput);
    public Task<TokenInfoDto> GetHolderTokenInfoAsync(string caAddress, string symbol);
}