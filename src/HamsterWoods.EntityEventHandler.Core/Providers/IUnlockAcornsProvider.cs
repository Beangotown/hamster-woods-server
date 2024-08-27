using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Rank;
using HamsterWoods.TokenLock;
using Nest;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.EntityEventHandler.Core.Providers;

public interface IUnlockAcornsProvider
{
    Task<List<RaceInfoConfigIndex>> GetRaceConfigAsync();
    Task<List<UserWeekRankRecordIndex>> GetRecordsAsync(int weekNum, int skip, int limit);
}

public class UnlockAcornsProvider : IUnlockAcornsProvider, ISingletonDependency
{
    private readonly INESTRepository<RaceInfoConfigIndex, string> _raceInfoRepository;
    private readonly INESTRepository<UserWeekRankRecordIndex, string> _userRecordRepository;

    public UnlockAcornsProvider(INESTRepository<RaceInfoConfigIndex, string> raceInfoRepository,
        INESTRepository<UserWeekRankRecordIndex, string> userRecordRepository)
    {
        _raceInfoRepository = raceInfoRepository;
        _userRecordRepository = userRecordRepository;
    }

    public async Task<List<RaceInfoConfigIndex>> GetRaceConfigAsync()
    {
        var result = await _raceInfoRepository.GetListAsync();
        return result.Item2;
    }

    public async Task<List<UserWeekRankRecordIndex>> GetRecordsAsync(int weekNum, int skip, int limit)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserWeekRankRecordIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.WeekNum).Value(weekNum)));
        QueryContainer Filter(QueryContainerDescriptor<UserWeekRankRecordIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await _userRecordRepository.GetSortListAsync(Filter, null,
            sortFunc: s => s.Ascending(a => a.Rank), skip: skip, limit: limit);
        return result.Item2;
    }
}