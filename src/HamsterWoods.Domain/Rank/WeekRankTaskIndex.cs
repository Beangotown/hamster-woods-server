using System;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Entities.Es;
using Nest;

namespace HamsterWoods.Rank;

public class WeekRankTaskIndex : HamsterWoodsEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }

    [Keyword] public string SeasonId { get; set; }

    public int? Week { get; set; }

    public bool IsFinished { get; set; }

    public DateTime TriggerTime { get; set; }
}