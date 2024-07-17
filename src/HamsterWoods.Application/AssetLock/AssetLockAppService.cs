using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.AssetLock.Dtos;
using HamsterWoods.AssetLock.Provider;
using HamsterWoods.Commons;
using HamsterWoods.Rank;
using HamsterWoods.Rank.Provider;
using HamsterWoods.TokenLock;
using Microsoft.Extensions.Logging;
using Nest;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace HamsterWoods.AssetLock;

[RemoteService(false), DisableAuditing]
public class AssetLockAppService : HamsterWoodsBaseService, IAssetLockAppService
{
    private readonly ILogger<AssetLockAppService> _logger;
    private readonly INESTRepository<UserWeekRankRecordIndex, string> _userRecordRepository;
    private readonly INESTRepository<RaceInfoConfigIndex, string> _raceInfoRepository;
    private readonly IRankProvider _rankProvider;
    private readonly IAssetLockProvider _assetLockProvider;

    public AssetLockAppService(ILogger<AssetLockAppService> logger,
        INESTRepository<UserWeekRankRecordIndex, string> userRecordRepository, IRankProvider rankProvider,
        IAssetLockProvider assetLockProvider, INESTRepository<RaceInfoConfigIndex, string> raceInfoRepository)
    {
        _logger = logger;
        _userRecordRepository = userRecordRepository;
        _rankProvider = rankProvider;
        _assetLockProvider = assetLockProvider;
        _raceInfoRepository = raceInfoRepository;
    }

    public async Task<AssetLockedInfoResultDto> GetLockedInfosAsync(GetAssetLockInfoDto input)
    {
        var lockedInfoList = new List<AssetLockedInfoDto>();
        var resultDto = new AssetLockedInfoResultDto()
        {
            LockedInfoList = lockedInfoList,
            TotalLockedAmount = 0,
            Decimals = CommonConstant.UsedTokenDecimals
        };
        var weekInfo = await _rankProvider.GetCurrentRaceInfoAsync();

        var unlockRecords = await _assetLockProvider.GetUnlockRecordsAsync(0, input.CaAddress,
            CommonConstant.DefaultSkipCount, CommonConstant.DefaultMaxResultCount);
        if (unlockRecords.UnLockRecordList.IsNullOrEmpty())
        {
            return resultDto;
        }

        var maxUnlockWeekNum = unlockRecords.UnLockRecordList.Max(t => t.WeekNum);
        var weekNums = new List<int>();
        for (var i = maxUnlockWeekNum + 1; i < weekInfo.WeekNum; i++)
        {
            weekNums.Add(i);
        }

        if (weekNums.Count == 0)
        {
            return resultDto;
        }

        var records = await GetRecordsAsync(weekNums, input.CaAddress);
        var raceInfos = await GetRaceConfigAsync(weekNums);
        foreach (var record in records)
        {
            var raceInfo = raceInfos.FirstOrDefault(t => t.WeekNum == record.WeekNum);
            if (raceInfo == null) continue;

            lockedInfoList.Add(new AssetLockedInfoDto()
            {
                Amount = record.SumScore,
                Decimals = record.Decimals,
                Symbol = CommonConstant.AcornsSymbol,
                LockedTime = raceInfo.SettleBeginTime.ToString("yyyy-MM-dd"),
                UnLockTime = raceInfo.SettleBeginTime.AddDays(raceInfo.AcornsLockedDays).ToString("yyyy-MM-dd")
            });
        }

        resultDto.TotalLockedAmount = lockedInfoList.Sum(t => t.Amount);
        return resultDto;
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
        
        var result = await _raceInfoRepository.GetListAsync(Filter);
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
                Symbol = CommonConstant.AcornsSymbol,
                Decimals = CommonConstant.UsedTokenDecimals,
                Amount = item.Amount,
                TransactionId = item.TransactionInfo?.TransactionId
            });
        }

        return result;
    }
}