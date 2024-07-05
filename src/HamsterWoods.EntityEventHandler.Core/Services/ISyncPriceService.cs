using System.Linq;
using System.Threading.Tasks;
using HamsterWoods.Cache;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.Options;
using HamsterWoods.Portkey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.EntityEventHandler.Core.Services;

public interface ISyncPriceService
{
    Task SyncPriceAsync();
}

public class SyncPriceService : ISyncPriceService, ISingletonDependency
{
    private readonly ILogger<SyncPriceService> _logger;
    private readonly IPortkeyProvider _portkeyProvider;
    private readonly ChainOptions _chainOptions;
    private readonly SyncPriceDataOptions _syncPriceDataOptions;

    public SyncPriceService(ILogger<SyncPriceService> logger, IPortkeyProvider portkeyProvider,
        IOptionsMonitor<ChainOptions> chainOptions,
        IOptionsMonitor<SyncPriceDataOptions> syncPriceDataOptions)
    {
        _logger = logger;
        _portkeyProvider = portkeyProvider;
        _chainOptions = chainOptions.CurrentValue;
        _syncPriceDataOptions = syncPriceDataOptions.CurrentValue;
    }

    public async Task SyncPriceAsync()
    {
        var sideChainId = _chainOptions.ChainInfos.Keys.First(t => t != "AELF");
        var elfInfo = await _portkeyProvider.GetHolderTokenInfoAsync(sideChainId, _syncPriceDataOptions.CaAddress,
            CommonConstant.ElfSymbol);

        decimal.TryParse(elfInfo.BalanceInUsd,  out var elfToUsd);
        var priceInfo = new PriceInfo()
        {
            ElfToUsd = elfToUsd,
        };
        
        
    }
}