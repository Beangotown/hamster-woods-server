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

    private const int QueryOnceLimit = 1000;
    private const string DateFormat = "yyyy-MM-dd";
    private const string StartTime = "00:00:00";
    private readonly ICacheProvider _cacheProvider;

    public RankService(INESTRepository<UserWeekRankIndex, string> userRankWeekRepository,
        INESTRepository<UserActionIndex, string> userActionRepository,
        IRankProvider rankProvider,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ChainOptions> chainOptions,
        IOptionsSnapshot<RaceOptions> raceOptions, ICacheProvider cacheProvider)
    {
        _userRankWeekRepository = userRankWeekRepository;
        _userActionRepository = userActionRepository;
        _objectMapper = objectMapper;
        _cacheProvider = cacheProvider;
        _rankProvider = rankProvider;
        _chainOptions = chainOptions.Value;
        _raceOptions = raceOptions.Value;
    }


    public async Task<WeekRankResultDto> GetWeekRankAsync(GetRankDto getRankDto)
    {
        var weekNum = 1; // should calculate
        var rankInfos = await _rankProvider.GetWeekRankAsync(weekNum, getRankDto.CaAddress, getRankDto.SkipCount,
            getRankDto.MaxResultCount);
        //var dayOfWeek = DateTime.UtcNow.DayOfWeek;
        SettleDaySelfRank settleDaySelfRank = null;
        if (true)
        {
            var settleDayRankingList = new List<SettleDayRank>();
            var selfBal = 0;
            var selfR = new NftInfo();
            if (rankInfos.SelfRank.Rank <= 10)
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

                settleDaySelfRank = new SettleDaySelfRank
                {
                    Score = rankInfos.SelfRank.Score,
                    CaAddress = rankInfos.SelfRank.CaAddress,
                    Decimals = 8,
                    Rank = rankInfos.SelfRank.Rank,
                    RewardNftInfo = selfR
                };
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
                        CaAddress = getRankDto.CaAddress,
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
        var weekNum = 1; // should calculate
        var rankInfos = await GetWeekRankAsync(input);
        if (rankInfos.SettleDaySelfRank == null)
        {
            return result;
        }

        var dto = new GetHistoryDto()
        {
            Time = "2024-1-07040705",
            CaAddress = input.CaAddress,
            Score = rankInfos.SettleDaySelfRank.Score,
            Decimals = 8,
            Rank = rankInfos.SettleDaySelfRank.Rank,
            RewardNftInfo = rankInfos.SettleDaySelfRank.RewardNftInfo
        };
        if (dto.RewardNftInfo != null)
        {
            var check = await CheckClaim(input.CaAddress);
            if (!check)
            {
                dto.RewardNftInfo = null;
            }
        }

        result.Add(dto);

        return result;
        // var dayOfWeek = DateTime.UtcNow.DayOfWeek;
        // //if (_raceOptions.SettleDayOfWeek == (int)dayOfWeek)


        // {
        //     return Task.FromResult(new List<GetHistoryDto>()
        //     {
        //         new GetHistoryDto()
        //         {
        //             Time = "2024-06-28",
        //             CaAddress = input.CaAddress,
        //             Score = 20000000000,
        //             Decimals = 8,
        //             Rank = 3,
        //             RewardNftInfo = new NftInfo()
        //             {
        //                 Balance = 5,
        //                 ChainId = "tDVW",
        //                 ImageUrl =
        //                     "https://forest-testnet.s3.ap-northeast-1.amazonaws.com/1008xAUTO/1718204324416-Activity%20Icon.png",
        //                 Symbol = "KINGPASS-1",
        //                 TokenName = "KINGPASS"
        //             }
        //         },
        //         new GetHistoryDto()
        //         {
        //             Time = "2024-06-21",
        //             CaAddress = input.CaAddress,
        //             Score = 230000000000,
        //             Decimals = 8,
        //             Rank = 2
        //         }
        //     });
        // }
    }

    private const string _hamsterPassCacheKeyPrefix = "HamsterKing_";
    private int weekNum = 1; // should cal

    public async Task<bool> CheckClaim(string caAddress)
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