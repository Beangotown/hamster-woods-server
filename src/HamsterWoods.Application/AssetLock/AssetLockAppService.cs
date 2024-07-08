using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.AssetLock.Dtos;
using HamsterWoods.AssetLock.Provider;
using HamsterWoods.Options;
using HamsterWoods.Rank;
using JetBrains.Annotations;
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
    private readonly IRankProvider _rankProvider;
    private readonly IAssetLockProvider _assetLockProvider;

    public AssetLockAppService(ILogger<AssetLockAppService> logger, IOptionsMonitor<RaceOptions> raceOptions,
        INESTRepository<UserWeekRankRecordIndex, string> userRecordRepository, IRankProvider rankProvider,
        IAssetLockProvider assetLockProvider)
    {
        _logger = logger;
        _userRecordRepository = userRecordRepository;
        _rankProvider = rankProvider;
        _assetLockProvider = assetLockProvider;
        _raceOptions = raceOptions.CurrentValue;
    }

    public async Task<AssetLockedInfoResultDto> GetLockedInfosAsync(GetAssetLockInfoDto input)
    {
        var lockedInfoList = new List<AssetLockedInfoDto>();
        // var weekNum = 2; // should calculate
        // var rankInfos = await _rankProvider.GetWeekRankAsync(weekNum, input.CaAddress, 0, 1);
        // if (rankInfos != null && rankInfos.SelfRank != null && rankInfos.SelfRank.Score > 0)
        // {
        //     var info = rankInfos.SelfRank;
        //     lockedInfoList.Add(new AssetLockedInfoDto()
        //     {
        //         Amount = info.Score,
        //         Decimals = 8,
        //         LockedTime = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"),
        //         Symbol = "ACORNS",
        //         UnLockTime = DateTime.UtcNow.AddDays(1).AddDays(-1).ToString("yyyy-MM-dd")
        //     });
        // }


        var rankInfos2 = await _rankProvider.GetWeekRankAsync(4, input.CaAddress, 0, 1);
        if (rankInfos2 != null && rankInfos2.SelfRank != null && rankInfos2.SelfRank.Score > 0)
        {
            lockedInfoList.Add(new AssetLockedInfoDto()
            {
                Amount = rankInfos2.SelfRank.Score,
                Decimals = 8,
                LockedTime = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Symbol = "ACORNS",
                UnLockTime = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd")
            });
        }

        var totalLockedAmount = lockedInfoList.Sum(t => t.Amount);
        // var weekNum = 1;
        // var weekNums = new List<int>() { 1, 2, 3, 4 };
        // var records = await GetRecordsAsync(weekNums, input.CaAddress);
        // var totalLockedAmount = records.Sum(t => t.SumScore);
        // int lockedDays = 30;
        //
        // var lockedInfoList = new List<AssetLockedInfoDto>();
        // foreach (var record in records)
        // {
        //     // next day as begin
        //     var date = _raceOptions.CalibrationTime.AddHours(_raceOptions.RaceHours * weekNum).AddHours(10);
        //     lockedInfoList.Add(new AssetLockedInfoDto()
        //     {
        //         Amount = record.SumScore,
        //         Decimals = 8,
        //         Symbol = "ACORNS",
        //         LockedTime = date.ToString("yyyy-MM-dd"),
        //         UnLockTime = date.AddDays(lockedDays).ToString("yyyy-MM-dd")
        //     });
        // }

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
        var mustQuery = new List<Func<QueryContainerDescriptor<UserWeekRankRecordIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CaAddress).Value(caAddress)));
        mustQuery.Add(q => q.Terms(i => i.Field(f => f.WeekNum).Terms(weekNums)));
        QueryContainer Filter(QueryContainerDescriptor<UserWeekRankRecordIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await _userRecordRepository.GetSortListAsync(Filter, null,
            sortFunc: s => s.Descending(a => a.WeekNum));

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