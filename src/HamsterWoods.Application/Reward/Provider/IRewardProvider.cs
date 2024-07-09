using System;
using System.Threading.Tasks;
using HamsterWoods.Cache;
using HamsterWoods.NFT;
using HamsterWoods.Options;
using HamsterWoods.Rank;
using HamsterWoods.Reward.Dtos;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Reward.Provider;

public interface IRewardProvider
{
    Task<TokenTransferInfo> GetRewardNftAsync(string caAddress, int weekNum);
    Task<KingHamsterClaimDto> IsClaimableAsync(string caAddress, int weekNum);
    Task<int> GetWeekNumAsync(int weekNum);
    Task<TokenTransferInfo> GetRewardNftAsync(RankDto selfRankDto);
    Task<TokenTransferInfo> GetCheckedRewardNftAsync(RankDto selfRankDto, int weekNum);
    int GetRewardNftBalance(int rank);
}

public class RewardProvider : IRewardProvider, ISingletonDependency
{
    private const string HamsterPassCacheKeyPrefix = "HamsterKing_";
    private readonly ICacheProvider _cacheProvider;
    private readonly IRankProvider _rankProvider;
    private readonly RewardNftInfoOptions _rewardNftInfoOptions;
    private readonly RaceOptions _raceOptions;

    public RewardProvider(ICacheProvider cacheProvider, IRankProvider rankProvider,
        IOptionsSnapshot<RewardNftInfoOptions> rewardNftInfoOptions,
        IOptionsSnapshot<RaceOptions> raceOptions)
    {
        _cacheProvider = cacheProvider;
        _rankProvider = rankProvider;
        _rewardNftInfoOptions = rewardNftInfoOptions.Value;
        _raceOptions = raceOptions.Value;
    }

    public async Task<KingHamsterClaimDto> IsClaimableAsync(string caAddress, int weekNum)
    {
        var weekInfo = await _rankProvider.GetCurrentRaceInfoAsync();
        var dayOfWeek = DateTime.UtcNow.DayOfWeek;

        var isSettleDay = _raceOptions.SettleDayOfWeeks.Contains((int)dayOfWeek);
        if (isSettleDay)
        {
            weekNum = weekNum - 1;
        }

        weekNum = await GetWeekNumAsync(weekNum);
        if (weekNum < weekInfo.WeekNum - 2)
        {
            return new KingHamsterClaimDto()
            {
                Claimable = false,
                Reason = "Reward expired."
            };
        }

        var isClaimed = await IsClaimedAsync(caAddress, weekNum);
        if (isClaimed)
        {
            return new KingHamsterClaimDto()
            {
                Claimable = false,
                Reason = ClaimBeanPassStatus.DoubleClaim.ToString()
            };
        }

        var nftInfo = await GetRewardNftAsync(caAddress, weekNum);
        if (nftInfo == null)
        {
            return new KingHamsterClaimDto()
            {
                Claimable = false,
                Reason = "Not allowed"
            };
        }

        return new KingHamsterClaimDto
        {
            Claimable = true,
            Reason = string.Empty
        };
    }

    public async Task<int> GetWeekNumAsync(int weekNum)
    {
        var weekInfo = await _rankProvider.GetCurrentRaceInfoAsync();
        var currentNum = weekInfo.WeekNum;
        if (weekNum == 0)
        {
            weekNum = currentNum - 1;
        }

        return weekNum;
    }

    private async Task<bool> IsClaimedAsync(string caAddress, int weekNum)
    {
        var passValue = await _cacheProvider.GetAsync($"{HamsterPassCacheKeyPrefix}{caAddress}_{weekNum}");
        return !passValue.IsNull;
    }

    public async Task<TokenTransferInfo> GetRewardNftAsync(string caAddress, int weekNum)
    {
        var selfInfo = await _rankProvider.GetSelfWeekRankAsync(weekNum, caAddress);
        if (selfInfo == null || selfInfo.Rank > 10 || selfInfo.Rank <= 0) return null;

        var nftBalance = GetRewardNftBalance(selfInfo.Rank);
        return new TokenTransferInfo
        {
            Amount = nftBalance,
            ChainId = _rewardNftInfoOptions.ChainId,
            Symbol = _rewardNftInfoOptions.Symbol
        };
    }

    public async Task<TokenTransferInfo> GetRewardNftAsync(RankDto selfRankDto)
    {
        if (selfRankDto == null || selfRankDto.Rank > 10 || selfRankDto.Rank <= 0) return null;

        var nftBalance = GetRewardNftBalance(selfRankDto.Rank);
        return new TokenTransferInfo
        {
            Amount = nftBalance,
            ChainId = _rewardNftInfoOptions.ChainId,
            Symbol = _rewardNftInfoOptions.Symbol
        };
    }

    public async Task<TokenTransferInfo> GetCheckedRewardNftAsync(RankDto selfRankDto, int weekNum)
    {
        var rewardInfo = await GetRewardNftAsync(selfRankDto);
        if (rewardInfo == null)
        {
            return null;
        }
        
        var isClaimed = await IsClaimedAsync(selfRankDto.CaAddress, weekNum);
        return isClaimed ? null : rewardInfo;
    }

    public int GetRewardNftBalance(int rank)
    {
        var nftBalance = 0;
        if (rank is > 10 or <= 0) return nftBalance;

        if (rank == 1) nftBalance = 3;
        if (rank == 2) nftBalance = 2;
        if (rank == 3) nftBalance = 2;
        if (rank > 3) nftBalance = 1;

        return nftBalance;
    }
}