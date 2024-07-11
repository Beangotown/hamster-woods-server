using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.NFT;
using HamsterWoods.Options;
using HamsterWoods.Rank.Provider;
using HamsterWoods.Reward.Provider;
using HamsterWoods.Trace;
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
    private readonly INESTRepository<UserActionIndex, string> _userActionRepository;
    private readonly IRankProvider _rankProvider;
    private readonly IObjectMapper _objectMapper;
    private readonly ChainOptions _chainOptions;
    private readonly RaceOptions _raceOptions;
    private readonly RewardNftInfoOptions _rewardNftInfoOptions;

    private const int QueryOnceLimit = 1000;
    private const string DateFormat = "yyyy-MM-dd";
    private const string StartTime = "00:00:00";
    private readonly IRewardProvider _rewardProvider;

    public RankService(INESTRepository<UserWeekRankRecordIndex, string> userRankWeekRepository,
        INESTRepository<UserActionIndex, string> userActionRepository,
        IRankProvider rankProvider,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ChainOptions> chainOptions,
        IOptionsSnapshot<RaceOptions> raceOptions,
        IOptionsSnapshot<RewardNftInfoOptions> rewardNftInfoOptions,
        IRewardProvider rewardProvider)
    {
        _userRankWeekRepository = userRankWeekRepository;
        _userActionRepository = userActionRepository;
        _objectMapper = objectMapper;
        _rewardProvider = rewardProvider;
        _rankProvider = rankProvider;
        _chainOptions = chainOptions.Value;
        _raceOptions = raceOptions.Value;
        _rewardNftInfoOptions = rewardNftInfoOptions.Value;
    }
    
    public async Task<WeekRankResultDto> GetWeekRankAsync(GetRankDto getRankDto)
    {
        var weekInfo = await _rankProvider.GetCurrentRaceInfoAsync();
        var weekNum = weekInfo.WeekNum;
        var dayOfWeek = DateTime.UtcNow.DayOfWeek;

        var isSettleDay = _raceOptions.SettleDayOfWeeks.Contains((int)dayOfWeek);
        if (isSettleDay)
        {
            weekNum = weekNum - 1;
        }

        var rankInfos = await _rankProvider.GetWeekRankAsync(weekNum, getRankDto.CaAddress, getRankDto.SkipCount,
            getRankDto.MaxResultCount);

        rankInfos.WeekNum = weekNum;
        if (!isSettleDay)
        {
            rankInfos.EndDate = weekInfo.CurrentRaceTimeInfo.EndTime.ToString("yyyy-MM-dd");
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
            SettleDaySelfRank = settleDaySelfRank
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

    public async Task SyncGameDataAsync()
    {
        var chainId = GetDefaultChainId();
        var userActionList = new List<UserActionIndex>();
        var goRecords = await _rankProvider.GetGoRecordsAsync();
        foreach (var record in goRecords)
        {
            var userActionIndex = new UserActionIndex();
            userActionIndex.CaAddress = AddressHelper.ToShortAddress(record.CaAddress);
            userActionIndex.ChainId = chainId;
            userActionIndex.Timestamp = record.TriggerTime;
            userActionIndex.Id =
                $"{userActionIndex.CaAddress}_{userActionIndex.ChainId}_{DateTimeHelper.ToUnixTimeMilliseconds(userActionIndex.Timestamp)}";
            userActionIndex.ActionType = UserActionType.Register;
            userActionList.Add(userActionIndex);
            if (userActionList.Count == QueryOnceLimit)
            {
                await _userActionRepository.BulkAddOrUpdateAsync(userActionList);
                userActionList.Clear();
            }
        }

        if (userActionList.Count > 0 && userActionList.Count < QueryOnceLimit)
        {
            await _userActionRepository.BulkAddOrUpdateAsync(userActionList);
            userActionList.Clear();
        }

        var startDate =
            DateTimeHelper.DatetimeToString(DateTime.UtcNow.AddDays(-Convert.ToInt32(DateTime.UtcNow.DayOfWeek) - 6),
                DateFormat);

        var dto = new GetGameHistoryDto();
        dto.BeginTime = DateTimeHelper.ParseDateTimeByStr($"{startDate} {StartTime}").AddHours(-8);
        dto.EndTime = DateTime.UtcNow.AddHours(1);
        dto.SkipCount = 0;
        dto.MaxResultCount = QueryOnceLimit;
        var historyRecords = new GameHisResultDto();
        do
        {
            historyRecords = await _rankProvider.GetGameHistoryListAsync(dto);
            if (historyRecords == null || historyRecords.GameList.IsNullOrEmpty()) break;

            foreach (var gameDto in historyRecords.GameList)
            {
                if (gameDto.BingoTransactionInfo == null || goRecords.Exists(r =>
                        r.CaAddress == gameDto.CaAddress && r.TriggerTime == gameDto.BingoTransactionInfo.TriggerTime))
                    continue;
                var userActionIndex = new UserActionIndex();
                userActionIndex.CaAddress = AddressHelper.ToShortAddress(gameDto.CaAddress);
                userActionIndex.ChainId = chainId;
                userActionIndex.Timestamp = gameDto.BingoTransactionInfo.TriggerTime;
                userActionIndex.Id =
                    $"{userActionIndex.CaAddress}_{userActionIndex.ChainId}_{DateTimeHelper.ToUnixTimeMilliseconds(userActionIndex.Timestamp)}";
                userActionIndex.ActionType = UserActionType.Login;
                userActionList.Add(userActionIndex);
            }

            await _userActionRepository.BulkAddOrUpdateAsync(userActionList);
            userActionList.Clear();
            dto.SkipCount += QueryOnceLimit;
        } while (historyRecords.GameList.Count >= QueryOnceLimit);
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

        // not sync last?
        
        var raceInfoList = await _rankProvider.GetRaceInfoAsync();
        var showCount = 0;
        foreach (var item in rankInfos)
        {
            var raceInfo = raceInfoList.FirstOrDefault(t => t.WeekNum == item.WeekNum);
            var beginStr = raceInfo.BeginTime.ToString("MMdd");
            var endStr = raceInfo.EndTime.ToString("MMdd");
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