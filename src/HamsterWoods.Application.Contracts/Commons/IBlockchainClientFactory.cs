using System.Collections.Concurrent;
using AElf.Client.Service;
using HamsterWoods.Contract;
using Microsoft.Extensions.Options;

namespace HamsterWoods.Commons;

public interface IBlockchainClientFactory<T> 
    where T : class
{
    T GetClient(string chainName);
}

public class AElfClientFactory : IBlockchainClientFactory<AElfClient>
{
    private readonly ChainOptions _options;
    private readonly ConcurrentDictionary<string, AElfClient> _clientDic;

    public AElfClientFactory(IOptionsSnapshot<ChainOptions> options)
    {
        _options = options.Value;
        _clientDic = new ConcurrentDictionary<string, AElfClient>();
    }

    public AElfClient GetClient(string chainId)
    {
        var chainInfo = _options.ChainInfos[chainId];
        if (_clientDic.TryGetValue(chainId, out var client))
        {
            return client;
        }

        client = new AElfClient(chainInfo.BaseUrl);
        _clientDic[chainId] = client;
        return client;
    }
}