using System.Collections.Concurrent;
using AElf.Client.Service;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Contract;

public class AElfClientFactory : ISingletonDependency
{
    private static readonly object _lock = new();
    private readonly ChainOptions _chainOptions;
    private readonly ConcurrentDictionary<string, AElfClient> _clientDictionary;

    public AElfClientFactory(IOptionsSnapshot<ChainOptions> chainOptions)
    {
        _chainOptions = chainOptions.Value;
        _clientDictionary = new ConcurrentDictionary<string, AElfClient>();
    }

    public AElfClient GetClient(string chainId)
    {
        if (!_chainOptions.ChainInfos.TryGetValue(chainId, out var chainInfo)) return null;
        if (_clientDictionary.TryGetValue(chainId, out var client)) return client;
        lock (_lock)
        {
            if (_clientDictionary.TryGetValue(chainId, out client)) return client;

            client = new AElfClient(chainInfo.BaseUrl);
            _clientDictionary[chainId] = client;
            return client;
        }
    }
}