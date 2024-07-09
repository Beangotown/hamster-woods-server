using System;
using System.Linq;
using System.Threading.Tasks;
using HamsterWoods.Cache;
using HamsterWoods.Contract;
using HamsterWoods.Options;
using HamsterWoods.Rank;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Common;

public interface IWeekNumProvider
{
    Task<int> GetWeekNumAsync(int weekNum);
    Task<CurrentRaceInfoCache> GetCurrentRaceInfoAsync();
}

public class WeekNumProvider : IWeekNumProvider, ISingletonDependency
{
    private readonly ChainOptions _chainOptions;
    private readonly ICacheProvider _cacheProvider;
    private readonly IContractProvider _contractProvider;

    public WeekNumProvider(IOptionsSnapshot<ChainOptions> chainOptions,
        ICacheProvider cacheProvider, IContractProvider contractProvider)
    {
        _cacheProvider = cacheProvider;
        _contractProvider = contractProvider;
        _chainOptions = chainOptions.Value;
    }

    public async Task<int> GetWeekNumAsync(int weekNum)
    {
        var weekInfo = await GetCurrentRaceInfoAsync();
        var currentNum = weekInfo.WeekNum;
        if (weekNum == 0)
        {
            weekNum = currentNum - 1;
        }

        return weekNum;
    }
    
    public async Task<CurrentRaceInfoCache> GetCurrentRaceInfoAsync()
    {
        var racePri = "CurrentRaceInfo";
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var cacheKey = $"{racePri}:{date}";
        cacheKey = cacheKey.Replace("-", ":");
        var cache = await _cacheProvider.Get<CurrentRaceInfoCache>(cacheKey);

        if (cache != null)
        {
            return cache;
        }
        
        var raceInfo = await _contractProvider.GetCurrentRaceInfoAsync(_chainOptions.ChainInfos.Keys.First());
        var raceCache = new CurrentRaceInfoCache
        {
            WeekNum = raceInfo.WeekNum,
            CurrentRaceTimeInfo = new CurrentRaceTimeInfo
            {
                BeginTime = raceInfo.RaceTimeInfo.BeginTime.ToDateTime(),
                EndTime = raceInfo.RaceTimeInfo.EndTime.ToDateTime(),
                SettleBeginTime = raceInfo.RaceTimeInfo.SettleBeginTime.ToDateTime(),
                SettleEndTime = raceInfo.RaceTimeInfo.SettleEndTime.ToDateTime()
            }
        };
        
        await _cacheProvider.Set<CurrentRaceInfoCache>(cacheKey, raceCache, null);
        return raceCache;
    }
}