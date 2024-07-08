using System.Threading.Tasks;
using HamsterWoods.Cache;
using HamsterWoods.NFT;
using HamsterWoods.Rank;
using HamsterWoods.Reward.Dtos;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Reward.Provider;

public interface IRewardProvider
{
    Task<TokenTransferInfo> GetRewardNftAsync(string caAddress, int weekNum);
    Task<KingHamsterClaimDto> IsClaimableAsync(string caAddress, int weekNum);
    int GetWeekNum(int weekNum);
}

public class RewardProvider : IRewardProvider, ISingletonDependency
{
    private const string HamsterPassCacheKeyPrefix = "HamsterKing_";
    private readonly ICacheProvider _cacheProvider;
    private readonly IRankProvider _rankProvider;

    public RewardProvider(ICacheProvider cacheProvider, IRankProvider rankProvider)
    {
        _cacheProvider = cacheProvider;
        _rankProvider = rankProvider;
    }

    public async Task<KingHamsterClaimDto> IsClaimableAsync(string caAddress, int weekNum)
    {
        // judge settle day
        var currentNum = 5;
        weekNum = GetWeekNum(weekNum);
        if (weekNum < currentNum - 2)
        {
            return new KingHamsterClaimDto()
            {
                Claimable = false,
                Reason = "Reward expired."
            };
        }

        var isClaimed = await IsClaimedAsync(caAddress, weekNum);
        if (!isClaimed)
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

    public int GetWeekNum(int weekNum)
    {
        var currentNum = 5;
        if (weekNum == 0)
        {
            weekNum = 4; // current weekNum -1
        }

        return weekNum;
    }

    private async Task<bool> IsClaimedAsync(string caAddress, int weekNum)
    {
        var passValue = await _cacheProvider.GetAsync($"{HamsterPassCacheKeyPrefix}{caAddress}_{weekNum}");
        return passValue.IsNull;
    }

    public async Task<TokenTransferInfo> GetRewardNftAsync(string caAddress, int weekNum)
    {
        var selfInfo = await _rankProvider.GetSelfWeekRankAsync(weekNum, caAddress);
        if (selfInfo == null || selfInfo.Rank > 10 || selfInfo.Rank <= 0) return null;

        var nftBalance = GetRewardNftBalance(selfInfo.Rank);
        return new TokenTransferInfo
        {
            Amount = nftBalance,
            ChainId = "tDVW",
            Symbol = "KINGHAMSTER-1"
        };
    }

    private int GetRewardNftBalance(int rank)
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