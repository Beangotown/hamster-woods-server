using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Common;
using HamsterWoods.Contract;
using HamsterWoods.NFT;
using HamsterWoods.Options;
using HamsterWoods.Rank.Provider;
using HamsterWoods.Reward.Provider;
using Microsoft.Extensions.Options;
using Nest;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Rank;

[RemoteService(false), DisableAuditing]
public class RankService : HamsterWoodsBaseService, IRankService
{
    private readonly INESTRepository<UserWeekRankRecordIndex, string> _userRankWeekRepository;
    private readonly IRankProvider _rankProvider;
    private readonly IObjectMapper _objectMapper;
    private readonly ChainOptions _chainOptions;
    private readonly RewardNftInfoOptions _rewardNftInfoOptions;
    private readonly IWeekNumProvider _weekNumProvider;
    private readonly IRewardProvider _rewardProvider;

    public RankService(INESTRepository<UserWeekRankRecordIndex, string> userRankWeekRepository,
        IRankProvider rankProvider,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ChainOptions> chainOptions,
        IOptionsSnapshot<RewardNftInfoOptions> rewardNftInfoOptions,
        IRewardProvider rewardProvider, IWeekNumProvider weekNumProvider)
    {
        _userRankWeekRepository = userRankWeekRepository;
        _objectMapper = objectMapper;
        _rewardProvider = rewardProvider;
        _weekNumProvider = weekNumProvider;
        _rankProvider = rankProvider;
        _chainOptions = chainOptions.Value;
        _rewardNftInfoOptions = rewardNftInfoOptions.Value;
    }
    
    public async Task<WeekRankResultDto> GetWeekRankAsync(GetRankDto getRankDto)
    {
        var weekInfo = await _rankProvider.GetCurrentRaceInfoAsync();
        var weekNumInfo = await _weekNumProvider.GetWeekNumInfoAsync();
        var weekNum = weekNumInfo.WeekNum;

        var rankInfos = await _rankProvider.GetWeekRankAsync(weekNum, getRankDto.CaAddress, getRankDto.SkipCount,
            getRankDto.MaxResultCount);

        rankInfos.WeekNum = weekNum;
        if (!weekNumInfo.IsSettleDay)
        {
            rankInfos.EndDate = weekInfo.CurrentRaceTimeInfo.EndTime.AddDays(-1).ToString("yyyy-MM-dd");
            return rankInfos;
        }

        var settleDayRankingList = new List<SettleDayRank>();

        NftInfo selfReward = null;
        var rewardInfo = await _rewardProvider.GetCheckedRewardNftAsync(rankInfos.SelfRank, weekNum);
        if (rewardInfo != null)
        {
            selfReward = _objectMapper.Map<RewardNftInfoOptions, NftInfo>(_rewardNftInfoOptions);
            selfReward.Balance = rewardInfo.Amount;
        }

        var settleDaySelfRank = new SettleDaySelfRank
        {
            Score = rankInfos.SelfRank.Score,
            CaAddress = rankInfos.SelfRank.CaAddress,
            Decimals = 8,
            Rank = rankInfos.SelfRank.Rank,
            RewardNftInfo = selfReward
        };

        foreach (var rankDto in rankInfos.RankingList.OrderBy(t => t.Rank).Take(3))
        {
            if (rankDto.Rank > 3) continue;
            var itemRank = new SettleDayRank()
            {
                FromRank = 0,
                ToRank = 0,
                CaAddress = rankDto.CaAddress,
                FromScore = 0,
                ToScore = 0,
                Rank = rankDto.Rank,
                Score = rankDto.Score,
                Decimals = 8,
                RewardNftInfo = _objectMapper.Map<RewardNftInfoOptions, NftInfo>(_rewardNftInfoOptions)
            };
            itemRank.RewardNftInfo.Balance = _rewardProvider.GetRewardNftBalance(rankDto.Rank);
            settleDayRankingList.Add(itemRank);
        }

        if (rankInfos.RankingList.Count <= 3)
        {
            return new WeekRankResultDto()
            {
                SettleDayRankingList = settleDayRankingList,
                SettleDaySelfRank = settleDaySelfRank
            };
        }

        var fromScore = rankInfos.RankingList[3].Score;
        var toScore = rankInfos.RankingList.Count > 10
            ? rankInfos.RankingList[9].Score
            : rankInfos.RankingList.OrderBy(t => t.Rank).Last().Score;
        var lastRank = new SettleDayRank()
        {
            FromRank = 4,
            ToRank = 10,
            CaAddress = getRankDto.CaAddress,
            FromScore = fromScore,
            ToScore = toScore,
            Rank = 0,
            Score = 0,
            Decimals = 8,
            RewardNftInfo = _objectMapper.Map<RewardNftInfoOptions, NftInfo>(_rewardNftInfoOptions)
        };
        lastRank.RewardNftInfo.Balance = _rewardProvider.GetRewardNftBalance(lastRank.FromRank);

        settleDayRankingList.Add(lastRank);
        return new WeekRankResultDto()
        {
            SettleDayRankingList = settleDayRankingList,
            SettleDaySelfRank = settleDaySelfRank,
            WeekNum = rankInfos.WeekNum
        };
    }

    private async Task<List<UserWeekRankDto>> GetWeekRankListAsync(string caAddress)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserWeekRankRecordIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CaAddress).Value(caAddress)));

        QueryContainer Filter(QueryContainerDescriptor<UserWeekRankRecordIndex> f)
        {
            return f.Bool(b => b.Must(mustQuery));
        }

        var result = await _userRankWeekRepository.GetSortListAsync(Filter, null,
            s => s.Descending(a => a.WeekNum)
        );

        return result.Item1 > 0
            ? _objectMapper.Map<List<UserWeekRankRecordIndex>, List<UserWeekRankDto>>(result.Item2)
            : new List<UserWeekRankDto>();
    }

    public async Task<List<GetHistoryDto>> GetHistoryAsync(GetRankDto input)
    {
        var result = new List<GetHistoryDto>();
        var rankInfos = (await GetWeekRankListAsync(input.CaAddress)).OrderByDescending(t => t.WeekNum).ToList();
        var weekInfo = await _rankProvider.GetCurrentRaceInfoAsync();
        var weekNum = weekInfo.WeekNum - 1;
        if (weekNum < 1)
        {
            return result;
        }
        
        var raceInfoList = await _rankProvider.GetRaceInfoAsync();
        var showCount = 0;
        foreach (var item in rankInfos)
        {
            var raceInfo = raceInfoList.FirstOrDefault(t => t.WeekNum == item.WeekNum);
            var beginStr = raceInfo.BeginTime.ToString("MMdd");
            var endStr = raceInfo.EndTime.AddDays(-1).ToString("MMdd");
            var dto = new GetHistoryDto
            {
                Time = $"2024-{item.WeekNum}-{beginStr}{endStr}",
                CaAddress = input.CaAddress,
                Score = item.SumScore,
                Decimals = 8,
                Rank = item.Rank,
                WeekNum = item.WeekNum
            };
            if (weekNum - item.WeekNum < 2)
            {
                NftInfo selfReward = null;
                var rewardInfo = await _rewardProvider.GetCheckedRewardNftAsync(new RankDto()
                {
                    CaAddress = item.CaAddress,
                    Rank = item.Rank,
                    Decimals = 8,
                    Score = item.SumScore
                }, item.WeekNum);
                if (rewardInfo != null)
                {
                    selfReward = _objectMapper.Map<RewardNftInfoOptions, NftInfo>(_rewardNftInfoOptions);
                    selfReward.Balance = rewardInfo.Amount;
                }

                dto.RewardNftInfo = selfReward;
            }

            result.Add(dto);
        }
        
        return result.OrderByDescending(t => t.WeekNum).ToList();
    }
    
    private string GetDefaultChainId()
    {
        return _chainOptions.ChainInfos.Keys.First();
    }
}