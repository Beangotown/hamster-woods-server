using System;
using System.Linq;
using System.Threading.Tasks;
using HamsterWoods.Cache;
using HamsterWoods.Contract;
using HamsterWoods.Rank;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Common;

public interface IWeekNumProvider
{
    Task<WeekNumInfo> GetWeekNumInfoAsync();
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

    public async Task<WeekNumInfo> GetWeekNumInfoAsync()
    {
        var weekNumInfo = new WeekNumInfo();
        var weekInfo = await GetCurrentRaceInfoAsync();
        weekNumInfo.WeekNum = weekInfo.WeekNum;
        if (weekInfo.CurrentRaceTimeInfo.BeginTime.Date == DateTime.UtcNow.Date && weekInfo.WeekNum > 1) //settle day
        {
            weekNumInfo.IsSettleDay = true;
            weekNumInfo.WeekNum -= 1;
        }

        return weekNumInfo;
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