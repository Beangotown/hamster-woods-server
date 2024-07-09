using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Cache;
using HamsterWoods.Commons;
using HamsterWoods.Contract;
using HamsterWoods.NFT;
using HamsterWoods.Options;
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
    private readonly INESTRepository<UserWeekRankIndex, string> _userRankWeekRepository;
    private readonly INESTRepository<UserActionIndex, string> _userActionRepository;
    private readonly IRankProvider _rankProvider;
    private readonly IObjectMapper _objectMapper;
    private readonly ChainOptions _chainOptions;
    private readonly RaceOptions _raceOptions;
    private readonly RewardNftInfoOptions _rewardNftInfoOptions;

    private const int QueryOnceLimit = 1000;
    private const string DateFormat = "yyyy-MM-dd";
    private const string StartTime = "00:00:00";
    private readonly ICacheProvider _cacheProvider;
    private readonly IRewardProvider _rewardProvider;

    public RankService(INESTRepository<UserWeekRankIndex, string> userRankWeekRepository,
        INESTRepository<UserActionIndex, string> userActionRepository,
        IRankProvider rankProvider,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ChainOptions> chainOptions,
        IOptionsSnapshot<RaceOptions> raceOptions,
        IOptionsSnapshot<RewardNftInfoOptions> rewardNftInfoOptions,
        ICacheProvider cacheProvider, IRewardProvider rewardProvider)
    {
        _userRankWeekRepository = userRankWeekRepository;
        _userActionRepository = userActionRepository;
        _objectMapper = objectMapper;
        _cacheProvider = cacheProvider;
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

        if (!isSettleDay) return rankInfos;
        
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

        var fromScore = rankInfos.RankingList[3].Score;
        var toScore = rankInfos.RankingList[9].Score;
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


    public async Task<WeekRankResultDto> GetWeekRankWeek1Async(GetRankDto getRankDto, int weekNum)
    {
        var rankInfos = await _rankProvider.GetWeekRankAsync(weekNum, getRankDto.CaAddress, getRankDto.SkipCount,
            getRankDto.MaxResultCount);
        //var dayOfWeek = DateTime.UtcNow.DayOfWeek;


        if (true)
        {
            var settleDayRankingList = new List<SettleDayRank>();
            var selfBal = 0;
            NftInfo selfR = null;
            if (rankInfos.SelfRank.Rank <= 10 && rankInfos.SelfRank.Rank > 0)
            {
                if (rankInfos.SelfRank.Rank == 1) selfBal = 3;
                if (rankInfos.SelfRank.Rank == 2) selfBal = 2;
                if (rankInfos.SelfRank.Rank == 3) selfBal = 2;
                if (rankInfos.SelfRank.Rank > 3) selfBal = 1;

                selfR = new NftInfo()
                {
                    Balance = selfBal,
                    ChainId = "tDVW",
                    ImageUrl =
                        "https://hamster-testnet.s3.ap-northeast-1.amazonaws.com/Acorns/NFT_KingHamster.png",
                    Symbol = "KINGHAMSTER-1",
                    TokenName = "King of Hamsters"
                };
            }

            var settleDaySelfRank = new SettleDaySelfRank
            {
                Score = rankInfos.SelfRank.Score,
                CaAddress = rankInfos.SelfRank.CaAddress,
                Decimals = 8,
                Rank = rankInfos.SelfRank.Rank,
                RewardNftInfo = selfR
            };

            if (settleDaySelfRank.RewardNftInfo != null)
            {
                var check = await CheckClaim(settleDaySelfRank.CaAddress, weekNum);
                if (!check)
                {
                    settleDaySelfRank.RewardNftInfo = null;
                }
            }

            var fromScore = rankInfos.RankingList[3].Score;
            var toScore = rankInfos.RankingList[9].Score;
            foreach (var rankDto in rankInfos.RankingList.OrderBy(t => t.Rank).Take(3))
            {
                if (rankDto.Rank <= 3)
                {
                    var balance = 0;
                    if (rankDto.Rank == 1) balance = 3;
                    if (rankDto.Rank == 2) balance = 2;
                    if (rankDto.Rank == 3) balance = 2;
                    settleDayRankingList.Add(new SettleDayRank()
                    {
                        FromRank = 0,
                        ToRank = 0,
                        CaAddress = rankDto.CaAddress,
                        FromScore = 0,
                        ToScore = 0,
                        Rank = rankDto.Rank,
                        Score = rankDto.Score,
                        Decimals = 8,
                        RewardNftInfo = new NftInfo()
                        {
                            Balance = balance,
                            ChainId = "tDVW",
                            ImageUrl =
                                "https://hamster-testnet.s3.ap-northeast-1.amazonaws.com/Acorns/NFT_KingHamster.png",
                            Symbol = "KINGHAMSTER-1",
                            TokenName = "King of Hamsters"
                        }
                    });
                }
            }

            settleDayRankingList.Add(new SettleDayRank()
            {
                FromRank = 4,
                ToRank = 10,
                CaAddress = getRankDto.CaAddress,
                FromScore = fromScore,
                ToScore = toScore,
                Rank = 0,
                Score = 0,
                Decimals = 8,
                RewardNftInfo = new NftInfo()
                {
                    Balance = 1,
                    ChainId = "tDVW",
                    ImageUrl =
                        "https://hamster-testnet.s3.ap-northeast-1.amazonaws.com/Acorns/NFT_KingHamster.png",
                    Symbol = "KINGHAMSTER-1",
                    TokenName = "King of Hamsters"
                }
            });
            return new WeekRankResultDto()
            {
                SettleDayRankingList = settleDayRankingList,
                SettleDaySelfRank = settleDaySelfRank
            };
        }

        return rankInfos;
    }

    private async Task<List<WeekRankDto>> GetWeekRankDtoListAsync(GetRankingHisDto getRankingHisDto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserWeekRankIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.SeasonId).Value(getRankingHisDto.SeasonId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CaAddress).Value(getRankingHisDto.CaAddress)));

        QueryContainer Filter(QueryContainerDescriptor<UserWeekRankIndex> f)
        {
            return f.Bool(b => b.Must(mustQuery));
        }

        var result = await _userRankWeekRepository.GetSortListAsync(Filter, null,
            s => s.Ascending(a => a.Week)
        );
        var weekRankDtos = new List<WeekRankDto>();
        foreach (var userWeekRankIndex in result.Item2)
        {
            var userWeekRank = _objectMapper.Map<UserWeekRankIndex, WeekRankDto>(userWeekRankIndex);
            weekRankDtos.Add(userWeekRank);
        }

        return weekRankDtos;
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
        var rankInfos = await GetWeekRankAsync(input);
        if (rankInfos.SettleDaySelfRank == null)
        {
            return result;
        }

        var dto = new GetHistoryDto()
        {
            Time = "2024-4-07070708",
            CaAddress = input.CaAddress,
            Score = rankInfos.SettleDaySelfRank.Score,
            Decimals = 8,
            Rank = rankInfos.SettleDaySelfRank.Rank,
            WeekNum = 4,
            RewardNftInfo = rankInfos.SettleDaySelfRank.RewardNftInfo
        };
        if (dto.RewardNftInfo != null)
        {
            var check = await CheckClaim(input.CaAddress, dto.WeekNum);
            if (!check)
            {
                dto.RewardNftInfo = null;
            }
        }

        if (dto.Score > 0)
        {
            result.Add(dto);
        }

        var his1 = await GetHistoryWeek1Async(input, 1, "2024-1-07040705");
        foreach (var item in his1)
        {
            item.RewardNftInfo = null;
        }

        result.AddRange(his1);
        var his2 = await GetHistoryWeek1Async(input, 2, "2024-2-07050706");
        foreach (var item in his2)
        {
            item.RewardNftInfo = null;
        }

        result.AddRange(his2);
        var his3 = await GetHistoryWeek1Async(input, 3, "2024-3-07060707");
        result.AddRange(his3);
        return result.Where(t => t.Score > 0).OrderByDescending(t => t.WeekNum).ToList();
    }

    public async Task<List<GetHistoryDto>> GetHistoryWeek1Async(GetRankDto input, int weekNum, string time)
    {
        var result = new List<GetHistoryDto>();
        var rankInfos = await GetWeekRankWeek1Async(input, weekNum);
        if (rankInfos.SettleDaySelfRank == null)
        {
            return result;
        }

        var dto = new GetHistoryDto()
        {
            Time = time,
            CaAddress = input.CaAddress,
            Score = rankInfos.SettleDaySelfRank.Score,
            Decimals = 8,
            Rank = rankInfos.SettleDaySelfRank.Rank,
            WeekNum = weekNum,
            RewardNftInfo = rankInfos.SettleDaySelfRank.RewardNftInfo
        };
        if (dto.RewardNftInfo != null)
        {
            var check = await CheckClaim(input.CaAddress, weekNum);
            if (!check)
            {
                dto.RewardNftInfo = null;
            }
        }

        result.Add(dto);

        return result;
    }

    private const string _hamsterPassCacheKeyPrefix = "HamsterKing_";

    public async Task<bool> CheckClaim(string caAddress, int weekNum)
    {
        var passValue = await _cacheProvider.GetAsync($"{_hamsterPassCacheKeyPrefix}{caAddress}_{weekNum}");
        if (!passValue.IsNull)
            return false;

        return true;
    }

    private string GetDefaultChainId()
    {
        return _chainOptions.ChainInfos.Keys.First();
    }
}