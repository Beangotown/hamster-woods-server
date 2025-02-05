using System;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Entities.Es;
using Nest;

namespace HamsterWoods.Rank;

public class UserWeekRankIndex : HamsterWoodsEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }

    [Keyword] public string SeasonId { get; set; }

    [Keyword] public string CaAddress { get; set; }
    public int Week { get; set; }
    public long SumScore { get; set; }

    public int Rank { get; set; }

    public DateTime UpdateTime { get; set; }
}