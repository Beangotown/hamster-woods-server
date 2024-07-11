using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using HamsterWoods.Cache;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.Portkey;
using HamsterWoods.Rank;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.NFT;

[RemoteService(false), DisableAuditing]
public class NFTService : HamsterWoodsBaseService, INFTService
{
    private const string _hamsterPassCacheKeyPrefix = "HamsterPass_";
    private readonly string _hamsterPassUsingCacheKeyPrefix = "HamsterPassUsing:";
    private readonly string _imageUrlKey = "__nft_image_url";
    private readonly ICacheProvider _cacheProvider;
    private readonly IContractProvider _contractProvider;
    private readonly IRankProvider _rankProvider;
    private readonly ILogger<NFTService> _logger;
    private readonly IPortkeyProvider _portkeyProvider;
    private readonly ChainOptions _chainOptions;
    private readonly IObjectMapper _objectMapper;

    public NFTService(
        IPortkeyProvider portkeyProvider,
        ICacheProvider cacheProvider,
        IContractProvider contractProvider,
        IRankProvider rankProvider,
        IOptionsSnapshot<ChainOptions> chainOptions, ILogger<NFTService> logger,
        IObjectMapper objectMapper)
    {
        _portkeyProvider = portkeyProvider;
        _cacheProvider = cacheProvider;
        _contractProvider = contractProvider;
        _rankProvider = rankProvider;
        _chainOptions = chainOptions.Value;
        _objectMapper = objectMapper;
        _logger = logger;
    }

    public async Task<HamsterPassDto> ClaimHamsterPassAsync(HamsterPassInput input)
    {
        _logger.LogInformation("ClaimBeanPassAsync {CaAddress}", input.CaAddress);
        var claimableDto = await IsHamsterPassClaimableAsync(input);
        if (!claimableDto.Claimable)
        {
            return new HamsterPassDto()
            {
                Claimable = claimableDto.Claimable,
                Reason = claimableDto.Reason
            };
        }

        var symbol = "HAMSTERPASS-1";
        var sendTransactionOutput = await _contractProvider.SendTransferAsync(symbol, "1",
            input.CaAddress,
            GetDefaultChainId()
        );

        await _cacheProvider.SetAsync(_hamsterPassCacheKeyPrefix + input.CaAddress,
            DateTime.UtcNow.ToTimestamp().ToString(),
            null);
        _logger.LogInformation("ClaimBeanPassAsync success {TransactionId}", sendTransactionOutput.TransactionId);
        var info = await GetHamsterPassInfoAsync(symbol);
        return new HamsterPassDto
        {
            Claimable = true,
            TransactionId = sendTransactionOutput.TransactionId,
            HamsterPassInfo = _objectMapper.Map<HamsterPassInfoDto, HamsterPassResultDto>(info) ??
                              new HamsterPassResultDto()
        };
    }

    public async Task<HamsterPassClaimableDto> IsHamsterPassClaimableAsync(HamsterPassInput input)
    {
        var passValue = await _cacheProvider.GetAsync(_hamsterPassCacheKeyPrefix + input.CaAddress);
        if (!passValue.IsNull)
            return new HamsterPassClaimableDto
            {
                Claimable = false,
                Reason = ClaimHamsterNftStatus.DoubleClaim.ToString()
            };

        return new HamsterPassClaimableDto
        {
            Claimable = true,
            Reason = string.Empty
        };
    }

    public async Task<List<HamsterPassResultDto>> GetNftListAsync(HamsterPassInput input)
    {
        var result = new List<HamsterPassResultDto>();

        var hamsterPasses = new List<string> { "HAMSTERPASS-1" };
        var balanceDto = new GetUserBalanceDto()
        {
            ChainId = GetDefaultChainId(),
            CaAddress = input.CaAddress,
            Symbols = hamsterPasses
        };
        var balanceList = (await _rankProvider.GetUserBalanceAsync(balanceDto))?.FindAll(b => b.Amount > 0);

        foreach (var hamsterPass in hamsterPasses)
        {
            var info = await GetHamsterPassInfoAsync(hamsterPass);
            var dto = _objectMapper.Map<HamsterPassInfoDto, HamsterPassResultDto>(info);
            var amount = balanceList?.FirstOrDefault(b => b.Symbol == hamsterPass)?.Amount ?? 0L;
            dto.Owned = amount > 0;
            dto.UsingBeanPass = true;
            result.Add(dto);
        }

        return result;
    }

    public async Task<bool> CheckHamsterPassAsync(HamsterPassInput input)
    {
        var balanceDto = new GetUserBalanceDto()
        {
            ChainId = GetDefaultChainId(),
            CaAddress = input.CaAddress,
            Symbols = new List<string> { "HAMSTERPASS-1" }
        };
        var balanceList = (await _rankProvider.GetUserBalanceAsync(balanceDto))?.FindAll(b => b.Amount > 0);
        return !balanceList.IsNullOrEmpty();
    }

    public async Task<List<TokenBalanceDto>> GetAssetAsync(HamsterPassInput input)
    {
        var caAddress = AddressHelper.ToShortAddress(input.CaAddress);
        var tokenInfos = new List<TokenBalanceDto>();

        var sideChainId = _chainOptions.ChainInfos.Keys.First(t => t != "AELF");
        var elfInfo = await _portkeyProvider.GetHolderTokenInfoAsync(sideChainId, caAddress, CommonConstant.ElfSymbol);
        tokenInfos.Add(new TokenBalanceDto()
        {
            Symbol = CommonConstant.ElfSymbol,
            Decimals = CommonConstant.UsedTokenDecimals,
            Balance = GetBalance(elfInfo.Balance)
        });

        var acornsInfo =
            await _portkeyProvider.GetHolderTokenInfoAsync(sideChainId, caAddress, CommonConstant.AcornsSymbol);
        tokenInfos.Add(new TokenBalanceDto()
        {
            Symbol = CommonConstant.AcornsSymbol,
            Decimals = CommonConstant.UsedTokenDecimals,
            Balance = GetBalance(acornsInfo.Balance)
        });

        return tokenInfos;
    }

    private long GetBalance(string balanceStr)
    {
        return !long.TryParse(balanceStr, out var balance) ? 0 : balance;
    }

    public async Task<PriceDto> GetPriceAsync()
    {
        var priceInfo = await _cacheProvider.Get<PriceInfo>(CommonConstant.DataPriceCacheKey);
        return _objectMapper.Map<PriceInfo, PriceDto>(priceInfo);
    }

    private async Task<HamsterPassInfoDto> GetHamsterPassInfoAsync(string symbol)
    {
        var key = $"{_hamsterPassCacheKeyPrefix}:{symbol}";
        var beanPassValue = await _cacheProvider.GetAsync(key);
        if (beanPassValue.IsNull)
        {
            var tokenInfo = await _contractProvider.GetTokenInfo(symbol, GetDefaultChainId());
            var info = new HamsterPassInfoDto()
            {
                Symbol = symbol,
                TokenName = tokenInfo.TokenName,
                NftImageUrl = tokenInfo.ExternalInfo.Value.TryGetValue(_imageUrlKey, out var url) ? url : null,
                TokenId = 1
            };
            await _cacheProvider.SetAsync(key, SerializeHelper.Serialize(info), null);
            return info;
        }

        return SerializeHelper.Deserialize<HamsterPassInfoDto>(beanPassValue);
    }

    private string GetDefaultChainId()
    {
        return _chainOptions.ChainInfos.Keys.First();
    }
}