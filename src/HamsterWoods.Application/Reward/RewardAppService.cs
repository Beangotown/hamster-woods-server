using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using HamsterWoods.Cache;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.NFT;
using HamsterWoods.Reward.Dtos;
using HamsterWoods.Reward.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Reward;

public class RewardAppService : IRewardAppService, ISingletonDependency
{
    private readonly ILogger<RewardAppService> _logger;
    private readonly ICacheProvider _cacheProvider;
    private readonly IObjectMapper _objectMapper;
    private readonly ChainOptions _chainOptions;
    private const string _hamsterPassCacheKeyPrefix = "HamsterKing_";
    private readonly string _imageUrlKey = "__nft_image_url";
    private readonly IContractProvider _contractProvider;
    private readonly IRewardProvider _rewardProvider;
    
    private readonly ISendAcornsProvider _sendAcornsProvider;

    public RewardAppService(ILogger<RewardAppService> logger,
        ICacheProvider cacheProvider,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ChainOptions> chainOptions, IContractProvider contractProvider,
        ISendAcornsProvider sendAcornsProvider, IRewardProvider rewardProvider)
    {
        _logger = logger;
        _cacheProvider = cacheProvider;
        _objectMapper = objectMapper;
        _contractProvider = contractProvider;
        _sendAcornsProvider = sendAcornsProvider;
        _rewardProvider = rewardProvider;
        _chainOptions = chainOptions.Value;
    }

    public async Task<KingHamsterClaimDto> ClaimHamsterKingAsync(HamsterPassInput input)
    {
        var caAddress = AddressHelper.ToFullAddress(input.CaAddress, GetDefaultChainId());
        var weekNum = await _rewardProvider.GetWeekNumAsync(input.WeekNum);
        var claimableDto = await _rewardProvider.IsClaimableAsync(caAddress, weekNum);
        if (!claimableDto.Claimable)
        {
            _logger.LogInformation(
                "[ClaimHamsterKingAsync] claimable is false, caAddress:{caAddress}, weekNum:{weekNum}, reason:{reason}",
                caAddress, weekNum, claimableDto.Reason);
            return claimableDto;
        }

        var transferInfo = await _rewardProvider.GetRewardNftAsync(caAddress, weekNum);

        _logger.LogInformation(
            "[ClaimHamsterKingAsync] begin send transaction, caAddress:{caAddress}, weekNum:{weekNum}, amount:{amount}",
            caAddress,
            weekNum, transferInfo.Amount.ToString());
        
        var sendTransactionOutput = await _contractProvider.SendTransferAsync(transferInfo.Symbol,
            transferInfo.Amount.ToString(),
            AddressHelper.ToShortAddress(input.CaAddress),
            GetDefaultChainId()
        );

        await _cacheProvider.SetAsync($"{_hamsterPassCacheKeyPrefix}{caAddress}_{weekNum}",
            DateTime.UtcNow.ToTimestamp().ToString(),
            null);
        _logger.LogInformation(
            "[ClaimHamsterKingAsync] success, caAddress:{caAddress}, weekNum:{weekNum}, transactionId:{transactionId}",
            caAddress,
            weekNum, sendTransactionOutput.TransactionId);

        var info = await GetHamsterPassInfoAsync(transferInfo.Symbol);
        return new KingHamsterClaimDto
        {
            Claimable = true,
            TransactionId = sendTransactionOutput.TransactionId,
            KingHamsterInfo = _objectMapper.Map<HamsterPassInfoDto, HamsterPassResultDto>(info) ??
                              new HamsterPassResultDto()
        };
    }

    public async Task<KingHamsterClaimDto> SendAsync(HamsterPassInput input)
    {
        return await _sendAcornsProvider.SendAsync(input);
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

    public async Task<HamsterPassClaimableDto> IsHamsterPassClaimableAsync(string caAddress, int weekNum)
    {
        var passValue = await _cacheProvider.GetAsync($"{_hamsterPassCacheKeyPrefix}{caAddress}_{weekNum}");
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

    private string GetDefaultChainId()
    {
        return _chainOptions.ChainInfos.Keys.First();
    }
}