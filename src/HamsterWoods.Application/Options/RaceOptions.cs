using System;

namespace HamsterWoods.Options;

public class RaceOptions
{
    public int RaceHours { get; set; }
    public DateTime CalibrationTime { get; set; }
    public int SettleDayOfWeek { get; set; } = 1;
}