using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Common;
using HamsterWoods.EntityEventHandler.Core.Providers;
using HamsterWoods.EntityEventHandler.Core.Services.Dtos;
using HamsterWoods.Rank;
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
    private readonly IWeekNumProvider _weekNumProvider;
    private readonly INESTRepository<UserWeekRankRecordIndex, string> _userRecordRepository;
    private readonly IObjectMapper _objectMapper;

    public SyncRankRecordService(ILogger<SyncRankRecordService> logger, ISyncRankRecordProvider syncRankRecordProvider,
        IWeekNumProvider weekNumProvider, INESTRepository<UserWeekRankRecordIndex, string> userRecordRepository,
        IObjectMapper objectMapper)
    {
        _logger = logger;
        _syncRankRecordProvider = syncRankRecordProvider;
        _weekNumProvider = weekNumProvider;
        _userRecordRepository = userRecordRepository;
        _objectMapper = objectMapper;
    }

    public async Task SyncRankRecordAsync()
    {
        // get data from graphql
        var weekNum = _weekNumProvider.GetWeekNum();
        var skipCount = 0;
        var maxResultCount = 10;
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
        records = records.OrderByDescending(t => t.SumScore).ThenByDescending(f => f.UpdateTime).ToList();
        foreach (var record in records)
        {
            record.Id = $"{record.CaAddress}-{record.WeekNum}";
            record.Rank = ++rank;
        }

        await _userRecordRepository.BulkAddOrUpdateAsync(records);
    }
}