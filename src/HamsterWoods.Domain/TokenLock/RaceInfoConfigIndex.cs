using System;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Entities.Es;
using Nest;

namespace HamsterWoods.TokenLock;

public class RaceInfoConfigIndex: HamsterWoodsEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    public int WeekNum { get; set; }
    public int AcornsLockedDays { get; set; }
    public DateTime BeginTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime SettleBeginTime { get; set; }
    public DateTime SettleEndTime { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
}