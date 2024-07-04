using System;
using HamsterWoods.Options;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Common;

public interface IWeekNumProvider
{
    int GetWeekNum();
}

public class WeekNumProvider : IWeekNumProvider, ISingletonDependency
{
    private readonly RaceOptions _raceOptions;

    public WeekNumProvider(IOptionsMonitor<RaceOptions> raceOptions)
    {
        _raceOptions = raceOptions.CurrentValue;
    }

    public int GetWeekNum()
    {
        var now = DateTime.UtcNow;
        var raceTime = _raceOptions.CalibrationTime.AddHours(_raceOptions.RaceHours);
        var weekNum = 1;
        while (now > raceTime)
        {
            raceTime = raceTime.AddHours(_raceOptions.RaceHours);
            weekNum++;
        }

        return weekNum;
    }
}