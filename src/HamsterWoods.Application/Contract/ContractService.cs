using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.Types;
using HamsterWoods.Cache;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Contract;

public class ContractService : IContractService, ISingletonDependency
{
    private readonly string _blockHeight = "blockheight:";
    private readonly ICacheProvider _cacheProvider;
    private readonly IContractProvider _contractProvider;
    private readonly ChainOptions _chainOptions;

    public ContractService(ICacheProvider cacheProvider,
        IContractProvider contractProvider,
        IOptionsSnapshot<ChainOptions> chainOptions)
    {
        _cacheProvider = cacheProvider;
        _contractProvider = contractProvider;
        _chainOptions = chainOptions.Value;
    }

    public async Task<long> GetBlockHeightAsync()
    {
        var defaultChainId = GetDefaultChainId();
        var value = await _cacheProvider.GetAsync(_blockHeight + defaultChainId);
        if (long.TryParse(value, out var blockHeight)) return blockHeight;
        var newBlockHeight = await _contractProvider.GetBlockHeightAsync(GetDefaultChainId());
        await _cacheProvider.SetAsync(_blockHeight + defaultChainId, newBlockHeight.ToString(),
            TimeSpan.FromMilliseconds(500));
        return newBlockHeight;
    }

    public async Task<Hash> GetRandomHashAsync()
    {
        var height = await GetBlockHeightAsync();
        return await _contractProvider.GetRandomHash(height, GetDefaultChainId());
    }

    private string GetDefaultChainId()
    {
        return _chainOptions.ChainInfos.Keys.First();
    }
}