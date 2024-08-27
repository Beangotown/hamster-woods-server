using System;
using System.Collections.Generic;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Entities.Es;
using Nest;

namespace HamsterWoods.TokenLock;

public class UnlockInfoIndex: HamsterWoodsEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    public int WeekNum { get; set; }
    [Keyword] public string BizId { get; set; }
    public List<string> Addresses { get; set; } = new();
    public DateTime CreateTime { get; set; }
}