using System;
using System.Collections.Generic;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Entities.Es;
using Nest;

namespace HamsterWoods.Rank;

public class RankSeasonConfigIndex : HamsterWoodsEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string? Name { get; set; }

    public int PlayerWeekRankCount { get; set; }
    public int PlayerWeekShowCount { get; set; }

    public int PlayerSeasonRankCount { get; set; }

    public int PlayerSeasonShowCount { get; set; }
    public DateTime RankBeginTime { get; set; }
    public DateTime RankEndTime { get; set; }
    public DateTime ShowBeginTime { get; set; }
    public DateTime ShowEndTime { get; set; }
    public List<RankWeekIndex> WeekInfos { get; set; }
}

public class RankWeekIndex
{
    public DateTime RankBeginTime { get; set; }
    public DateTime RankEndTime { get; set; }
    public DateTime ShowBeginTime { get; set; }
    public DateTime ShowEndTime { get; set; }
}