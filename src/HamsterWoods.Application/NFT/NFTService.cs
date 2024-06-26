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
    private const string _beanPassCacheKeyPrefix = "BeanPass_";
    private const string _beanPassLimitCacheKeyPrefix = "BeanPassLimit_";
    private readonly string _beanPassUsingCacheKeyPrefix = "BeanPassUsing:";
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

    private const string BeanPopPassCacheKeyPrefix = "BeanPassPop:";
    private const string BeanPassCurrentlyNot = "This BeanPass NFT is currently not in your account.";
    private const string BeanPassNoHave = "You don't have any BeanPass NFTs in your account.";
    

    public NFTService(IPortkeyProvider portkeyProvider, ICacheProvider cacheProvider,
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
        _userActivityOptions = userActivityOptions.Value;
        _halloweenActivityOptions = halloweenActivityOptions.Value;
        _chainOptions = chainOptions.Value;
        _contractService = contractService;
        _objectMapper = objectMapper;
        _logger = logger;
    }

    public async Task<HamsterPassDto> ClaimBeanPassAsync(HamsterPassInput input)
    {
        _logger.LogInformation("ClaimBeanPassAsync {CaAddress}", input.CaAddress);
        var beanPass = await IsBeanPassClaimableAsync(input);
        if (!beanPass.Claimable) return beanPass;
        if (beanPass.Reason.Equals(ClaimBeanPassStatus.NewUser.ToString()))
        {
            var value = await _cacheProvider.IncreaseAsync(_beanPassLimitCacheKeyPrefix + DateTime.UtcNow.Day, 1,
                TimeSpan.FromDays(2));
            if (value > _userActivityOptions.ClaimCountPerDay)
                return new HamsterPassDto
                {
                    Claimable = false,
                    Reason = ClaimBeanPassStatus.Claimed.ToString()
                };
        }

        var symbol = await GetRandomSymbolAsync();
        var sendTransactionOutput = await _contractProvider.SendTransferAsync(symbol, "1",
            input.CaAddress,
            GetDefaultChainId()
        );
        
        
        await _cacheProvider.SetAsync(_beanPassCacheKeyPrefix + input.CaAddress,
            DateTime.UtcNow.ToTimestamp().ToString(),
            null);
        _logger.LogInformation("ClaimBeanPassAsync success {TransactionId}", sendTransactionOutput.TransactionId);
        var info = await GetBeanPassInfoAsync(symbol);
        return new HamsterPassDto
        {
            Claimable = true,
            TransactionId = sendTransactionOutput.TransactionId,
            BeanPassInfoDto = _objectMapper.Map<BeanPassInfoDto, BeanPassResultDto>(info) ?? new BeanPassResultDto()
        };
    }

    public async Task<HamsterPassDto> IsBeanPassClaimableAsync(HamsterPassInput input)
    {
        var beanPassValue = await _cacheProvider.GetAsync(_beanPassCacheKeyPrefix + input.CaAddress);
        if (!beanPassValue.IsNull)
            return new HamsterPassDto
            {
                Claimable = false,
                Reason = ClaimBeanPassStatus.DoubleClaim.ToString()
            };
        var balance = await _portkeyProvider.GetTokenBalanceAsync(input);
        if (balance >= _userActivityOptions.NeedElfAmount)
            return new HamsterPassDto
            {
                Claimable = true,
                Reason = ClaimBeanPassStatus.ElfAmountEnough.ToString()
            };
        var time = await _portkeyProvider.GetCaHolderCreateTimeAsync(input);
        if (time == 0)
            throw new UserFriendlyException(BeangoTownConstants.SyncingMessage, BeangoTownConstants.SyncingCode);
        var begin = DateTimeHelper.ParseDateTimeByStr(_userActivityOptions.BeginTime);
        var end = DateTimeHelper.ParseDateTimeByStr(_userActivityOptions.EndTime);
        var createTime = DateTimeHelper.FromUnixTimeSeconds(time);
        if (begin.CompareTo(createTime) == -1 && createTime.CompareTo(end) == -1)
        {
            var value = await _cacheProvider.GetAsync(_beanPassLimitCacheKeyPrefix + DateTime.UtcNow.Day);
            if (int.TryParse(value, out var count) && count > _userActivityOptions.ClaimCountPerDay)
                return new HamsterPassDto
                {
                    Claimable = false,
                    Reason = ClaimBeanPassStatus.Claimed.ToString()
                };
            return new HamsterPassDto
            {
                Claimable = true,
                Reason = ClaimBeanPassStatus.NewUser.ToString()
            };
        }

        return new HamsterPassDto
        {
            Claimable = false,
            Reason = ClaimBeanPassStatus.InsufficientElfAmount.ToString()
        };
    }

    public async Task<List<BeanPassResultDto>> GetNftListAsync(HamsterPassInput input)
    {
        var result = new List<BeanPassResultDto>();

        // now  before
        var begin = DateTimeHelper.ParseDateTimeByStr(_halloweenActivityOptions.BeginTime);
        var beanPassList = begin.CompareTo(DateTime.UtcNow) > 0 ? new List<string>{ _userActivityOptions.BeanPass } : _halloweenActivityOptions.BeanPass;
        var balanceDto = new GetUserBalanceDto()
        {
            ChainId = GetDefaultChainId(),
            CaAddress = input.CaAddress,
            Symbols = new List<string>()
        };
        var balanceList = (await _rankProvider.GetUserBalanceAsync(balanceDto))?.FindAll(b => b.Amount > 0);
        var beanPassValue = await _cacheProvider.GetAsync($"{_beanPassUsingCacheKeyPrefix}{input.CaAddress}");
        var symbol = string.Empty;
        if (!balanceList.IsNullOrEmpty() && balanceList.Count > 0)
        {
            symbol = balanceList.Count == 1 ? balanceList[0].Symbol : beanPassValue;
            if (symbol.IsNullOrEmpty())
            {
                symbol = _halloweenActivityOptions.BeanPass[0];
            }
        }

        foreach (var beanPass in beanPassList)
        {
            var info = await GetBeanPassInfoAsync(beanPass);
            var dto = _objectMapper.Map<BeanPassInfoDto, BeanPassResultDto>(info);
            
            var amount = balanceList?.FirstOrDefault(b => b.Symbol == beanPass)?.Amount ?? 0L;
            dto.Owned = amount > 0;
            dto.UsingBeanPass = dto.Owned && beanPass == symbol;
            if (dto.UsingBeanPass)
            {
                await _cacheProvider.SetAsync($"{_beanPassUsingCacheKeyPrefix}{input.CaAddress}", beanPass, null);
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task<BeanPassResultDto> UsingBeanPassAsync(GetHamsterPassInput input)
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

        var info = await GetBeanPassInfoAsync(input.Symbol);
        var dto = _objectMapper.Map<BeanPassInfoDto, BeanPassResultDto>(info) ?? new BeanPassResultDto();
        dto.Owned = true;
        dto.UsingBeanPass = true;
        var key = $"{_beanPassUsingCacheKeyPrefix}{input.CaAddress}";
        await _cacheProvider.SetAsync(key, input.Symbol, null);
        return dto;
    }

    public async Task<bool> CheckBeanPassAsync(HamsterPassInput input)
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

    private async Task<long> GetAmountAsync(string caAddress, string symbol)
    {
        var balanceDto = new GetUserBalanceDto()
        {
            ChainId = GetDefaultChainId(),
            CaAddress = caAddress,
            Symbols = new List<string>(){ symbol }
        };
        var balanceList = await _rankProvider.GetUserBalanceAsync(balanceDto);
        return balanceList?.FirstOrDefault()?.Amount ?? 0L;
    }

    private async Task<BeanPassInfoDto> GetBeanPassInfoAsync(string symbol)
    {
        var key = $"{_beanPassCacheKeyPrefix}:{symbol}";
        var beanPassValue = await _cacheProvider.GetAsync(key);
        if (beanPassValue.IsNull)
        {
            var tokenInfo = await _contractProvider.GetTokenInfo(symbol, GetDefaultChainId());
            var info = new BeanPassInfoDto()
            {
                Symbol = symbol,
                TokenName = tokenInfo.TokenName,
                NftImageUrl = tokenInfo.ExternalInfo.Value.TryGetValue(_imageUrlKey, out var url) ? url : null
            };
            await _cacheProvider.SetAsync(key, SerializeHelper.Serialize(info), null);
            return info;
        }

        return SerializeHelper.Deserialize<BeanPassInfoDto>(beanPassValue);
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

        var claimTime = await _cacheProvider.GetAsync(_beanPassCacheKeyPrefix + input.CaAddress);
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

    private async Task<string> GetRandomSymbolAsync()
    {
        var dictCount = 3;
        var beginTime = DateTimeHelper.ParseDateTimeByStr(_halloweenActivityOptions.BeginTime);
        var endTime = DateTimeHelper.ParseDateTimeByStr(_halloweenActivityOptions.EndTime);
        if (DateTime.UtcNow.CompareTo(beginTime) > -1 && DateTime.UtcNow.CompareTo(endTime) == -1)
        {
            var randomHash = await _contractService.GetRandomHashAsync();

            var randomList = GetDices(randomHash, dictCount);
            var random = randomList[RandomHelper.GetRandom(dictCount)];
            var symbol = _halloweenActivityOptions.BeanPass[random];
            return symbol;
        }

        return _userActivityOptions.BeanPass;
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