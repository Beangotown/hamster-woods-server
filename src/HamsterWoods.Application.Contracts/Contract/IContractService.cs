using System.Threading.Tasks;
using AElf.Types;

namespace HamsterWoods.Contract;

public interface IContractService
{
    public Task<long> GetBlockHeightAsync();

    public Task<Hash> GetRandomHashAsync();
}