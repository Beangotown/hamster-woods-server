using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
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

    public RankService(INESTRepository<UserWeekRankIndex, string> userRankWeekRepository,
        INESTRepository<UserActionIndex, string> userActionRepository,
        IRankProvider rankProvider,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ChainOptions> chainOptions,
        IOptionsSnapshot<RaceOptions> raceOptions)
    {
        _userRankWeekRepository = userRankWeekRepository;
        _userActionRepository = userActionRepository;
        _objectMapper = objectMapper;
        _rankProvider = rankProvider;
        _chainOptions = chainOptions.Value;
        _raceOptions = raceOptions.Value;
    }


    public async Task<WeekRankResultDto> GetWeekRankAsync(GetRankDto getRankDto)
    {
        var weekNum = 1; // should calculate
        var rankInfos = await _rankProvider.GetWeekRankAsync(weekNum, getRankDto.CaAddress, getRankDto.SkipCount,
            getRankDto.MaxResultCount);
        var dayOfWeek = DateTime.UtcNow.DayOfWeek;
        if (_raceOptions.SettleDayOfWeek == (int)dayOfWeek)
        {
            return new WeekRankResultDto()
            {
                SettleDayRankingList = new List<SettleDayRank>()
                {
                    new SettleDayRank()
                    {
                        FromRank = 4,
                        ToRank = 6,
                        CaAddress = getRankDto.CaAddress,
                        FromScore = 56700000000,
                        ToScore = 89900000000,
                        Rank = 5,
                        Score = 78700000000,
                        Decimals = 8,
                        RewardNftInfo = new NftInfo()
                        {
                            Balance = 5,
                            ChainId = "tDVW",
                            ImageUrl =
                                "https://forest-testnet.s3.ap-northeast-1.amazonaws.com/1008xAUTO/1718204324416-Activity%20Icon.png",
                            Symbol = "KINGPASS-1",
                            TokenName = "KINGPASS"
                        }
                    }
                },
                SettleDaySelfRank = new SettleDaySelfRank()
                {
                    CaAddress = getRankDto.CaAddress,
                    Rank = 5,
                    Score = 78700000000,
                    Decimals = 8,
                    RewardNftInfo = new NftInfo()
                    {
                        Balance = 5,
                        ChainId = "tDVW",
                        ImageUrl =
                            "https://forest-testnet.s3.ap-northeast-1.amazonaws.com/1008xAUTO/1718204324416-Activity%20Icon.png",
                        Symbol = "KINGPASS-1",
                        TokenName = "KINGPASS"
                    }
                }
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

    public Task<List<GetHistoryDto>> GetHistoryAsync(GetRankDto input)
    {
        var dayOfWeek = DateTime.UtcNow.DayOfWeek;
        //if (_raceOptions.SettleDayOfWeek == (int)dayOfWeek)
        {
            return Task.FromResult(new List<GetHistoryDto>()
            {
                new GetHistoryDto()
                {
                    Time = "2024-06-28",
                    CaAddress = input.CaAddress,
                    Score = 20000000000,
                    Decimals = 8,
                    Rank = 3,
                    RewardNftInfo = new NftInfo()
                    {
                        Balance = 5,
                        ChainId = "tDVW",
                        ImageUrl =
                            "https://forest-testnet.s3.ap-northeast-1.amazonaws.com/1008xAUTO/1718204324416-Activity%20Icon.png",
                        Symbol = "KINGPASS-1",
                        TokenName = "KINGPASS"
                    }
                },
                new GetHistoryDto()
                {
                    Time = "2024-06-21",
                    CaAddress = input.CaAddress,
                    Score = 230000000000,
                    Decimals = 8,
                    Rank = 2
                }
            });
        }
    }
    

    private string GetDefaultChainId()
    {
        return _chainOptions.ChainInfos.Keys.First();
    }
}