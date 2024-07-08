using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using HamsterWoods.Cache;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.NFT;
using HamsterWoods.Rank;
using HamsterWoods.Reward.Dtos;
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

    private readonly IRankService _rankService;
    private readonly ISendAcornsProvider _sendAcornsProvider;

    public RewardAppService(ILogger<RewardAppService> logger,
        ICacheProvider cacheProvider,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ChainOptions> chainOptions, IContractProvider contractProvider, IRankService rankService, ISendAcornsProvider sendAcornsProvider)
    {
        _logger = logger;
        _cacheProvider = cacheProvider;
        _objectMapper = objectMapper;
        _contractProvider = contractProvider;
        _rankService = rankService;
        _sendAcornsProvider = sendAcornsProvider;
        _chainOptions = chainOptions.Value;
    }

    public async Task<KingHamsterClaimDto> ClaimHamsterKingAsync(HamsterPassInput input)
    {
        var weekNum = input.WeekNum;
        if (weekNum == 0)
        {
            weekNum = 3; // current weekNum
        }

        var caAddress = AddressHelper.ToFullAddress(input.CaAddress, GetDefaultChainId());
        var week = await _rankService.GetWeekRankAsync(new GetRankDto()
        {
            CaAddress = caAddress,
            MaxResultCount = 10,
            SkipCount = 0
        });

        if (week.SettleDaySelfRank == null || week.SettleDaySelfRank.Rank > 10 ||
            week.SettleDaySelfRank.RewardNftInfo == null)
        {
            return new KingHamsterClaimDto()
            {
                Claimable = false,
                Reason = "Not allowed"
            };
        }

        var amount = week.SettleDaySelfRank.RewardNftInfo.Balance;

        _logger.LogInformation("ClaimHamsterKingAsync {CaAddress}", caAddress);
        var claimableDto = await IsHamsterPassClaimableAsync(caAddress, weekNum);
        if (!claimableDto.Claimable)
        {
            return new KingHamsterClaimDto()
            {
                Claimable = claimableDto.Claimable,
                Reason = claimableDto.Reason
            };
        }

        var symbol = "KINGHAMSTER-1";
        var sendTransactionOutput = await _contractProvider.SendTransferAsync(symbol, amount.ToString(),
            AddressHelper.ToShortAddress("2f9rumzM1roxHb748W1Dkgx2mB1D7hqTpYQNyctn5A2S2d6yhS"),
            GetDefaultChainId()
        );

        await _cacheProvider.SetAsync($"{_hamsterPassCacheKeyPrefix}{caAddress}_{weekNum}",
            DateTime.UtcNow.ToTimestamp().ToString(),
            null);
        _logger.LogInformation("ClaimBeanPassAsync success {TransactionId}", sendTransactionOutput.TransactionId);
        var info = await GetHamsterPassInfoAsync(symbol);
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