using System;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Entities.Es;
using Nest;

namespace HamsterWoods.Points;

public class HopRecordIndex : HamsterWoodsEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string CaAddress { get; set; }
    public DateTime TriggerTime { get; set; }
    [Keyword] public string TransactionId { get; set; }
}