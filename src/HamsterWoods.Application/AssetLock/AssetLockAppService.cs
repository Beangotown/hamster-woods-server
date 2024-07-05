using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.AssetLock.Dtos;
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

    public AssetLockAppService(ILogger<AssetLockAppService> logger, IOptionsMonitor<RaceOptions> raceOptions,
        INESTRepository<UserWeekRankRecordIndex, string> userRecordRepository, IRankProvider rankProvider)
    {
        _logger = logger;
        _userRecordRepository = userRecordRepository;
        _rankProvider = rankProvider;
        _raceOptions = raceOptions.CurrentValue;
    }

    public async Task<AssetLockedInfoResultDto> GetLockedInfosAsync(GetAssetLockInfoDto input)
    {
        var weekNum = 1; // should calculate
        var rankInfos = await _rankProvider.GetWeekRankAsync(weekNum, input.CaAddress, 0, 1);
        if (rankInfos == null || rankInfos.SelfRank == null || rankInfos.SelfRank.Score == 0)
        {
            return new AssetLockedInfoResultDto()
            {
                Decimals = 8
            };
        }

        var lockedInfoList = new List<AssetLockedInfoDto>();
        var info = rankInfos.SelfRank;
        lockedInfoList.Add(new AssetLockedInfoDto()
        {
            Amount = info.Score,
            Decimals = 8,
            LockedTime = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            Symbol = "ACORNS",
            UnLockTime = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd")
        });

        var totalLockedAmount = info.Score;
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

    public Task<List<GetUnlockRecordDto>> GetUnlockRecordsAsync(GetAssetLockInfoDto input)
    {
        return Task.FromResult(new List<GetUnlockRecordDto>()
        {
            // new GetUnlockRecordDto()
            // {
            //     UnLockTime = "2024-07-24",
            //     Symbol = "ACORNS",
            //     Decimals = 8,
            //     Amount = 10000000000,
            //     TransactionId = "685fa94f58d5176438b678ebf317fc23fb6539adc66127c6221b7a18a4a20364"
            // }
        });
    }
}