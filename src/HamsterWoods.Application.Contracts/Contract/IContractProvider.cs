using System.Threading.Tasks;
using AElf.Client.Dto;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Contracts.HamsterWoods;
using Google.Protobuf;

namespace HamsterWoods.Contract;

public interface IContractProvider
{
    public Task<GetBalanceOutput> GetBalanceAsync(string symbol, string address, string chainId);
    public Task<SendTransactionOutput> SendTransferAsync(string symbol, string amount, string address, string chainId);

    public Task<long> GetBlockHeightAsync(string chainId);
    
    public Task<Hash> GetRandomHash(long targetHeight, string chainId);
    
    public Task<TokenInfo> GetTokenInfo(string symbol, string chainId);
    Task<CurrentRaceInfo> GetCurrentRaceInfoAsync(string chainId);

    Task<T> CallTransactionAsync<T>(string chainId, string contractAddress, string methodName,
        IMessage param
    ) where T : class, IMessage<T>, new();
}