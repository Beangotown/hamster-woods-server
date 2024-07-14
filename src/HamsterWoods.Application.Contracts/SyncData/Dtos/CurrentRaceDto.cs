using System;

namespace HamsterWoods.SyncData.Dtos;

public class CurrentRaceDto
{
    public string Id { get; set; }
    public int WeekNum { get; set; }
    public int AcornsLockedDays { get; set; }
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime SettleBeginTime { get; set; }
    public DateTime SettleEndTime { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
}