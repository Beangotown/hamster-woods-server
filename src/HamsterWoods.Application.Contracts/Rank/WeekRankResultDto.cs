using System.Collections.Generic;
using JetBrains.Annotations;

namespace HamsterWoods.Rank;

public class WeekRankResultGraphDto
{
    public WeekRankResultDto GetWeekRank { get; set; }
}

public class WeekRankRecordDto
{
    public WeekRankResultDto GetWeekRankRecords { get; set; }
}


public class WeekRankResultDto
{
    [CanBeNull] public List<RankDto> RankingList { get; set; }
    [CanBeNull] public List<SettleDayRank> SettleDayRankingList { get; set; }
    [CanBeNull] public RankDto SelfRank { get; set; }
    [CanBeNull] public SettleDaySelfRank SettleDaySelfRank { get; set; }
}

public class SelfWeekRankGraphQlDto
{
    [CanBeNull] public RankDto GetSelfWeekRank { get; set; }
}