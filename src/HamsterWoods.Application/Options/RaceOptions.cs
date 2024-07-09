using System;
using System.Collections.Generic;

namespace HamsterWoods.Options;

public class RaceOptions
{
    public int RaceHours { get; set; }
    public DateTime CalibrationTime { get; set; }
    public List<int> SettleDayOfWeeks { get; set; }
}