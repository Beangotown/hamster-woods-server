using System.Collections.Generic;

namespace HamsterWoods.Rank;

public class RankingHisResultGraphDto
{
    public RankingHisResultDto GetRankingHistory { get; set; }
}
public class RankingHisResultDto
{
    public RankDto Season { get; set; }
    public List<WeekRankDto> Weeks { get; set; }
}