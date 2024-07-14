using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Google.Protobuf.WellKnownTypes;
using HamsterWoods.Cache;
using HamsterWoods.Common;
using HamsterWoods.EntityEventHandler.Core.Providers;
using HamsterWoods.EntityEventHandler.Core.Services.Dtos;
using HamsterWoods.Rank;
using HamsterWoods.SyncData;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.EntityEventHandler.Core.Services;

public interface ISyncRankRecordService
{
    Task SyncRankRecordAsync();
}

public class SyncRankRecordService : ISyncRankRecordService, ISingletonDependency
{
    private readonly ILogger<SyncRankRecordService> _logger;
    private readonly ISyncRankRecordProvider _syncRankRecordProvider;
    private readonly INESTRepository<UserWeekRankRecordIndex, string> _userRecordRepository;
    private readonly IObjectMapper _objectMapper;
    private readonly ISyncDataService _syncDataService;

    public SyncRankRecordService(ILogger<SyncRankRecordService> logger,
        ISyncRankRecordProvider syncRankRecordProvider,
        INESTRepository<UserWeekRankRecordIndex, string> userRecordRepository,
        IObjectMapper objectMapper, ISyncDataService syncDataService)
    {
        _logger = logger;
        _syncRankRecordProvider = syncRankRecordProvider;
        _userRecordRepository = userRecordRepository;
        _objectMapper = objectMapper;
        _syncDataService = syncDataService;
    }

    public async Task SyncRankRecordAsync()
    {
        _logger.LogInformation("[SyncRankRecord] SyncRankRecord Start.");
        var currentRaceDto = await _syncDataService.SyncRaceConfigAsync();

        var weekNum = currentRaceDto.WeekNum;
        if (weekNum == 1)
        {
            _logger.LogInformation("[SyncRankRecord] weekNum:{weekNum}, first week race", weekNum);
            return;
        }

        if (currentRaceDto.BeginTime.Date != DateTime.UtcNow.Date)
        {
            _logger.LogInformation("[SyncRankRecord] weekNum:{weekNum}, beginTime:{beginTime}, not settle day", weekNum,
                currentRaceDto.BeginTime.ToString("yyyy-MM-dd HH:mm:ss"));
            return;
        }

        weekNum = currentRaceDto.WeekNum - 1; // last weekNum
        _logger.LogInformation("[SyncRankRecord] weekNum:{weekNum}", weekNum);
        await SyncRecordAsync(weekNum);
        _logger.LogInformation("[SyncRankRecord] SyncRankRecord End.");
    }

    private async Task SyncRecordAsync(int weekNum)
    {
        var skipCount = 0;
        var maxResultCount = 100;
        var result = await _syncRankRecordProvider.GetWeekRankRecordsAsync(weekNum, skipCount, maxResultCount);
        await SaveRecordAsync(result?.RankRecordList, skipCount);

        while (result != null && !result.RankRecordList.IsNullOrEmpty())
        {
            skipCount += maxResultCount;
            result = await _syncRankRecordProvider.GetWeekRankRecordsAsync(weekNum, skipCount, maxResultCount);
            await SaveRecordAsync(result?.RankRecordList, skipCount);
        }
    }

    private async Task SaveRecordAsync(List<RankRecordDto> recordList, int rankNum)
    {
        if (recordList.IsNullOrEmpty()) return;

        var rank = rankNum;
        var records = _objectMapper.Map<List<RankRecordDto>, List<UserWeekRankRecordIndex>>(recordList);

        records = records.OrderByDescending(t => t.SumScore).ThenBy(f => f.UpdateTime).ToList();
        foreach (var record in records)
        {
            record.Id = $"{record.CaAddress}-{record.WeekNum}";
            record.Rank = ++rank;
        }

        await _userRecordRepository.BulkAddOrUpdateAsync(records);
    }
}