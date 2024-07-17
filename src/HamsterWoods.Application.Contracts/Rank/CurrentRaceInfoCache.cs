using System;

namespace HamsterWoods.Rank;

public class CurrentRaceInfoCache
{
    public int WeekNum { get; set; }
    public CurrentRaceTimeInfo CurrentRaceTimeInfo { get; set; }
}

public class CurrentRaceTimeInfo
{
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime SettleBeginTime { get; set; }
    public DateTime SettleEndTime { get; set; }
}