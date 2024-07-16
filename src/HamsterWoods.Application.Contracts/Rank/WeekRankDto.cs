using System;

namespace HamsterWoods.Rank;

public class WeekRankDto
{
    public int Week { get; set; }
    public string CaAddress { get; set; }
    public long Score { get; set; }
    public int Rank { get; set; }
}

public class UserWeekRankDto
{
    public string CaAddress { get; set; }
    public long SumScore { get; set; }
    public string Symbol { get; set; }
    public int Decimals { get; set; }
    public int WeekNum { get; set; }
    public int Rank { get; set; }
    public DateTime UpdateTime { get; set; }
}