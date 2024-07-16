using System;
using System.Linq;
using System.Threading.Tasks;
using Awaken.Contracts.Swap;
using HamsterWoods.Cache;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.Options;
using HamsterWoods.Portkey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
    private readonly IContractProvider _contractProvider;
    private readonly ICacheProvider _cacheProvider;

    public SyncPriceService(ILogger<SyncPriceService> logger, IPortkeyProvider portkeyProvider,
        IOptionsMonitor<ChainOptions> chainOptions,
        IOptionsMonitor<SyncPriceDataOptions> syncPriceDataOptions, IContractProvider contractProvider,
        ICacheProvider cacheProvider)
    {
        _logger = logger;
        _portkeyProvider = portkeyProvider;
        _contractProvider = contractProvider;
        _cacheProvider = cacheProvider;
        _chainOptions = chainOptions.CurrentValue;
        _syncPriceDataOptions = syncPriceDataOptions.CurrentValue;
    }

    public async Task SyncPriceAsync()
    {
        var sideChainId = _chainOptions.ChainInfos.Keys.First(t => t != "AELF");
        var elfInfo = await _portkeyProvider.GetHolderTokenInfoAsync(sideChainId, _syncPriceDataOptions.CaAddress,
            CommonConstant.ElfSymbol);

        decimal.TryParse(elfInfo.BalanceInUsd, out var elfToUsd);
        var priceInfo = new PriceInfo()
        {
            ElfToUsd = elfToUsd,
        };

        var param = new GetReservesInput()
        {
            SymbolPair = { "ACORNS-ELF" }
        };

        var output = await _contractProvider.CallTransactionAsync<GetReservesOutput>(sideChainId,
            _chainOptions.ChainInfos[sideChainId].AwakenAddress, AElfConstants.GetReserves, param);

        var reservePairResult = output.Results.FirstOrDefault();
        if (reservePairResult == null)
        {
            _logger.LogError("[SyncPrice] GetReserves from awaken is null");
        }

        priceInfo.AcornsToElf = (decimal)reservePairResult.ReserveB / (decimal)reservePairResult.ReserveA;
        priceInfo.AcornsToUsd = priceInfo.ElfToUsd * priceInfo.AcornsToElf;
        priceInfo.AcornsToElf = Math.Round(priceInfo.AcornsToElf, 8);
        priceInfo.AcornsToUsd = Math.Round(priceInfo.AcornsToUsd, 8);

        _logger.LogInformation("[SyncPrice] sync price data success, data:{data}",
            JsonConvert.SerializeObject(priceInfo));
        // save in redis
        await _cacheProvider.Set(CommonConstant.DataPriceCacheKey, priceInfo, null);
    }
}