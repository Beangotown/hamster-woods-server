using System;
using System.Collections.Generic;

namespace HamsterWoods.Rank;

public class SeasonRankResultGraphDto
{
    public SeasonRankResultDto GetSeasonRank { get; set; }
}

public class SeasonRankRecordDto
{
    public SeasonRankResultDto GetSeasonRankRecords { get; set; }
}


public class SeasonRankResultDto
{
    public string SeasonName { get; set; }
    public int Status { get; set; }
    public DateTime? RefreshTime { get; set; }
    public List<RankDto> RankingList { get; set; }
    public RankDto SelfRank { get; set; }
}