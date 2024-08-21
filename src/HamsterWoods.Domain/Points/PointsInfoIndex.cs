using System;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Entities.Es;
using Nest;

namespace HamsterWoods.Points;

public class PointsInfoIndex: HamsterWoodsEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string ChainId { get; set; }
    [Keyword] public string Address { get; set; }
    public long Amount { get; set; }
    [Keyword] public string PointType { get; set; }
    public DateTime CreateTime { get; set; }
    [Keyword] public string BizId { get; set; }
    [Keyword] public string ContractInvokeStatus { get; set; }
}