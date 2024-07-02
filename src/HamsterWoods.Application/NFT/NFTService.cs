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
    // for test
    private const string _hamsterPassCacheKeyPrefix = "TestPass_";


    //private const string _hamsterPassCacheKeyPrefix = "HamsterPass_";
    private readonly string _hamsterPassUsingCacheKeyPrefix = "HamsterPassUsing:";
    private readonly string _imageUrlKey = "__nft_image_url";
    private readonly ICacheProvider _cacheProvider;
    private readonly IContractProvider _contractProvider;
    private readonly IRankProvider _rankProvider;
    private readonly ILogger<NFTService> _logger;
    private readonly UserActivityOptions _userActivityOptions;
    private readonly HalloweenActivityOptions _halloweenActivityOptions;
    private readonly IPortkeyProvider _portkeyProvider;
    private readonly ChainOptions _chainOptions;
    private readonly IContractService _contractService;
    private readonly IObjectMapper _objectMapper;

    private const string BeanPopPassCacheKeyPrefix = "HamsterPassPop:";
    private const string BeanPassCurrentlyNot = "This HamsterPass NFT is currently not in your account.";
    private const string BeanPassNoHave = "You don't have any HamsterPass NFTs in your account.";


    public NFTService(
        IPortkeyProvider portkeyProvider,
        ICacheProvider cacheProvider,
        IContractProvider contractProvider,
        IContractService contractService,
        IRankProvider rankProvider,
        IOptionsSnapshot<UserActivityOptions> userActivityOptions,
        IOptionsSnapshot<HalloweenActivityOptions> halloweenActivityOptions,
        IOptionsSnapshot<ChainOptions> chainOptions, ILogger<NFTService> logger,
        IObjectMapper objectMapper)
    {
        _portkeyProvider = portkeyProvider;
        _cacheProvider = cacheProvider;
        _contractProvider = contractProvider;
        _rankProvider = rankProvider;
        _contractService = contractService;
        _userActivityOptions = userActivityOptions.Value;
        _halloweenActivityOptions = halloweenActivityOptions.Value;
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

        //var symbol = "HAMSTERPASS-1";

        // for test
        var symbol = "TTZZ-1";

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
                Reason = ClaimBeanPassStatus.DoubleClaim.ToString()
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

        var beanPassList = new List<string> { "TTZZ-1" };
        var balanceDto = new GetUserBalanceDto()
        {
            ChainId = GetDefaultChainId(),
            CaAddress = input.CaAddress,
            Symbols = new List<string>()
        };
        var balanceList = (await _rankProvider.GetUserBalanceAsync(balanceDto))?.FindAll(b => b.Amount > 0);

        foreach (var beanPass in beanPassList)
        {
            var info = await GetHamsterPassInfoAsync(beanPass);
            var dto = _objectMapper.Map<HamsterPassInfoDto, HamsterPassResultDto>(info);
            var amount = balanceList?.FirstOrDefault(b => b.Symbol == beanPass)?.Amount ?? 0L;
            dto.Owned = amount > 0;
            result.Add(dto);
        }

        return result;
    }

    public async Task<HamsterPassResultDto> UsingBeanPassAsync(GetHamsterPassInput input)
    {
        if (!_halloweenActivityOptions.BeanPass.Contains(input.Symbol))
        {
            throw new UserFriendlyException(BeanPassCurrentlyNot);
        }

        var amount = await GetAmountAsync(input.CaAddress, input.Symbol);
        if (amount == 0)
        {
            throw new UserFriendlyException(BeanPassCurrentlyNot);
        }

        var info = await GetHamsterPassInfoAsync(input.Symbol);
        var dto = _objectMapper.Map<HamsterPassInfoDto, HamsterPassResultDto>(info) ?? new HamsterPassResultDto();
        dto.Owned = true;
        dto.UsingBeanPass = true;
        var key = $"{_hamsterPassUsingCacheKeyPrefix}{input.CaAddress}";
        await _cacheProvider.SetAsync(key, input.Symbol, null);
        return dto;
    }

    public async Task<bool> CheckHamsterPassAsync(HamsterPassInput input)
    {
        var balanceDto = new GetUserBalanceDto()
        {
            ChainId = GetDefaultChainId(),
            CaAddress = input.CaAddress,
            Symbols = _halloweenActivityOptions.BeanPass
        };
        var balanceList = (await _rankProvider.GetUserBalanceAsync(balanceDto))?.FindAll(b => b.Amount > 0);
        return !balanceList.IsNullOrEmpty();
    }

    public async Task<List<TokenBalanceDto>> GetAssetAsync(HamsterPassInput input)
    {
        var tokenInfos = new List<TokenBalanceDto>();

        var elfBalance = await _portkeyProvider.GetTokenBalanceAsync(input.CaAddress, "ELF");
        tokenInfos.Add(new TokenBalanceDto()
        {
            Symbol = "ELF",
            Decimals = 8,
            Balance = elfBalance
        });
        // var balance = await _portkeyProvider.GetTokenBalanceAsync(input.CaAddress, "ACORNS");

        tokenInfos.Add(new TokenBalanceDto()
        {
            Symbol = "ACORNS",
            Decimals = 8,
            Balance = 0
        });

        return tokenInfos;
    }

    public Task<PriceDto> GetPriceAsync()
    {
        return Task.FromResult(new PriceDto()
        {
            AcornsInElf = 0.1m,
            ElfInUsd = 0.35m
        });
    }

    private async Task<long> GetAmountAsync(string caAddress, string symbol)
    {
        var balanceDto = new GetUserBalanceDto()
        {
            ChainId = GetDefaultChainId(),
            CaAddress = caAddress,
            Symbols = new List<string>() { symbol }
        };
        var balanceList = await _rankProvider.GetUserBalanceAsync(balanceDto);
        return balanceList?.FirstOrDefault()?.Amount ?? 0L;
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

    public async Task<bool> PopupBeanPassAsync(HamsterPassInput input)
    {
        var beginTime = _halloweenActivityOptions.BeginTime;
        var endTime = _halloweenActivityOptions.EndTime;
        var beginDateTime = DateTimeHelper.ParseDateTimeByStr(beginTime);
        var endDateTime = DateTimeHelper.ParseDateTimeByStr(endTime);
        var dateUtcTime = DateTime.UtcNow;
        _logger.LogDebug("PopupBeanPass beginTime :{beginTime} endTime:{endTime}", beginTime, endTime);

        if (dateUtcTime.CompareTo(beginDateTime) < 0 || dateUtcTime.CompareTo(endDateTime) > 0)
        {
            return false;
        }

        var claimTime = await _cacheProvider.GetAsync(_hamsterPassCacheKeyPrefix + input.CaAddress);
        _logger.LogDebug("PopupBeanPass claimTime :{claimTime} caAddress:{caAddress}", claimTime, input.CaAddress);


        if (claimTime.IsNullOrEmpty)
        {
            return false;
        }

        var timeStr1 = claimTime.ToString().Replace("\"", "");
        var lastDotIndex = timeStr1.LastIndexOf('.');
        var result = timeStr1.Substring(0, lastDotIndex);
        _logger.LogDebug("PopupBeanPass result time :{result} caAddress:{caAddress}", result, input.CaAddress);

        var claimDateTime = DateTime.ParseExact(result, "yyyy-MM-dd'T'HH:mm:ss", null);
        if (claimDateTime.CompareTo(beginDateTime) > 0)
        {
            return false;
        }

        var popValue = await _cacheProvider.GetAsync(BeanPopPassCacheKeyPrefix + input.CaAddress);
        if (popValue.IsNullOrEmpty)
        {
            var beanPassPopKey = BeanPopPassCacheKeyPrefix + input.CaAddress;
            var beanPassPopValue = dateUtcTime.ToString();
            _logger.LogDebug("PopupBeanPassAsync key:{key} ,value{value}", beanPassPopKey, beanPassPopValue);
            await _cacheProvider.SetAsync(beanPassPopKey, beanPassPopValue, null);
            return true;
        }
        else
        {
            return false;
        }
    }

    private string GetDefaultChainId()
    {
        return _chainOptions.ChainInfos.Keys.First();
    }

    private List<int> GetDices(Hash hashValue, int diceCount)
    {
        var hexString = hashValue.ToHex();
        var dices = new List<int>();

        for (var i = 0; i < diceCount; i++)
        {
            var startIndex = i * 8;
            var intValue = int.Parse(hexString.Substring(startIndex, 8),
                NumberStyles.HexNumber);
            var dice = Math.Abs(intValue % 2);
            dices.Add(dice);
        }

        return dices;
    }
}