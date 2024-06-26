using System.Threading.Tasks;
using HamsterWoods.NFT;

namespace HamsterWoods.Portkey;

public interface IPortkeyProvider
{
    public Task<long> GetCaHolderCreateTimeAsync(BeanPassInput beanPassInput);
    public Task<long> GetTokenBalanceAsync(BeanPassInput beanPassInput);
}