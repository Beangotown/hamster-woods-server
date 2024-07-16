using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.AssetLock.Dtos;
using HamsterWoods.AssetLock.Provider;
using HamsterWoods.Options;
using HamsterWoods.Rank;
using HamsterWoods.Rank.Provider;
using HamsterWoods.TokenLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace HamsterWoods.AssetLock;

[RemoteService(false), DisableAuditing]
public class AssetLockAppService : HamsterWoodsBaseService, IAssetLockAppService
{
    private readonly ILogger<AssetLockAppService> _logger;
    private readonly RaceOptions _raceOptions;
    private readonly INESTRepository<UserWeekRankRecordIndex, string> _userRecordRepository;
    private readonly INESTRepository<RaceInfoConfigIndex, string> raceInfoRepository;
    private readonly IRankProvider _rankProvider;
    private readonly IAssetLockProvider _assetLockProvider;

    public AssetLockAppService(ILogger<AssetLockAppService> logger, IOptionsMonitor<RaceOptions> raceOptions,
        INESTRepository<UserWeekRankRecordIndex, string> userRecordRepository, IRankProvider rankProvider,
        IAssetLockProvider assetLockProvider, INESTRepository<RaceInfoConfigIndex, string> raceInfoRepository)
    {
        _logger = logger;
        _userRecordRepository = userRecordRepository;
        _rankProvider = rankProvider;
        _assetLockProvider = assetLockProvider;
        this.raceInfoRepository = raceInfoRepository;
        _raceOptions = raceOptions.CurrentValue;
    }

    public async Task<AssetLockedInfoResultDto> GetLockedInfosAsync(GetAssetLockInfoDto input)
    {
        var lockedInfoList = new List<AssetLockedInfoDto>();
        var weekInfo = await _rankProvider.GetCurrentRaceInfoAsync();

        var unlockRecords = await _assetLockProvider.GetUnlockRecordsAsync(0, input.CaAddress, 0, 1000);
        var maxUnlockWeekNum = unlockRecords.UnLockRecordList.Max(t => t.WeekNum);
        var weekNums = new List<int>();
        for (var i = maxUnlockWeekNum + 1; i < weekInfo.WeekNum; i++)
        {
            weekNums.Add(i);
        }

        //var records = await GetRecordsAsync(weekNums, input.CaAddress);
        if (weekNums.Count == 0)
        {
            return new AssetLockedInfoResultDto()
            {
                LockedInfoList = lockedInfoList,
                TotalLockedAmount = 0,
                Decimals = 8
            };
        }
        var weekNum = weekNums.First();
        var records = await _rankProvider.GetWeekRankAsync(weekNum, input.CaAddress,0,1);
        var raceInfos = await GetRaceConfigAsync(weekNums);
        if (records.SelfRank == null || records.SelfRank.Score == 0)
        {
            return new AssetLockedInfoResultDto()
            {
                LockedInfoList = lockedInfoList,
                TotalLockedAmount = 0,
                Decimals = 8
            };
        }
        var raceInfo = raceInfos.FirstOrDefault(t => t.WeekNum == weekNum);
        lockedInfoList.Add(new AssetLockedInfoDto()
        {
            //Amount = record.SumScore,
            Amount = records.SelfRank.Score,
            Decimals = records.SelfRank.Decimals,
            Symbol = "ACORNS",
            LockedTime = raceInfo.SettleBeginTime.ToString("yyyy-MM-dd"),
            UnLockTime = raceInfo.SettleBeginTime.AddDays(raceInfo.AcornsLockedDays).ToString("yyyy-MM-dd")
        });
        // foreach (var record in records.RankingList)
        // {
        //     var raceInfo = raceInfos.FirstOrDefault(t => t.WeekNum == record.WeekNum);
        //     if (raceInfo == null) continue;
        //
        //     lockedInfoList.Add(new AssetLockedInfoDto()
        //     {
        //         //Amount = record.SumScore,
        //         Amount = record.Score,
        //         Decimals = record.Decimals,
        //         Symbol = "ACORNS",
        //         LockedTime = raceInfo.SettleBeginTime.ToString("yyyy-MM-dd"),
        //         UnLockTime = raceInfo.SettleBeginTime.AddDays(raceInfo.AcornsLockedDays).ToString("yyyy-MM-dd")
        //     });
        // }

        var totalLockedAmount = lockedInfoList.Sum(t => t.Amount);
        var result = new AssetLockedInfoResultDto
        {
            LockedInfoList = lockedInfoList,
            TotalLockedAmount = totalLockedAmount,
            Decimals = 8
        };
        return result;
    }

    private async Task<List<UserWeekRankRecordIndex>> GetRecordsAsync(List<int> weekNums, string caAddress)
    {
        if (weekNums.IsNullOrEmpty()) return new List<UserWeekRankRecordIndex>();

        var mustQuery = new List<Func<QueryContainerDescriptor<UserWeekRankRecordIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CaAddress).Value(caAddress)));
        mustQuery.Add(q => q.Terms(i => i.Field(f => f.WeekNum).Terms(weekNums)));
        QueryContainer Filter(QueryContainerDescriptor<UserWeekRankRecordIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await _userRecordRepository.GetSortListAsync(Filter, null,
            sortFunc: s => s.Descending(a => a.WeekNum));

        return result.Item2;
    }

    private async Task<List<RaceInfoConfigIndex>> GetRaceConfigAsync(List<int> weekNums)
    {
        if (weekNums.IsNullOrEmpty()) return new List<RaceInfoConfigIndex>();

        var mustQuery = new List<Func<QueryContainerDescriptor<RaceInfoConfigIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Terms(i => i.Field(f => f.WeekNum).Terms(weekNums)));
        QueryContainer Filter(QueryContainerDescriptor<RaceInfoConfigIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await raceInfoRepository.GetListAsync(Filter);

        return result.Item2;
    }

    public async Task<List<GetUnlockRecordDto>> GetUnlockRecordsAsync(GetAssetLockInfoDto input)
    {
        var dto = await _assetLockProvider.GetUnlockRecordsAsync(0, input.CaAddress, input.SkipCount,
            input.MaxResultCount);

        var result = new List<GetUnlockRecordDto>();
        foreach (var item in dto.UnLockRecordList)
        {
            result.Add(new GetUnlockRecordDto()
            {
                UnLockTime = item.BlockTime.ToString("yyyy-MM-dd"),
                Symbol = "ACORNS",
                Decimals = 8,
                Amount = item.Amount,
                TransactionId = item.TransactionInfo?.TransactionId
            });
        }

        return result;
    }
}