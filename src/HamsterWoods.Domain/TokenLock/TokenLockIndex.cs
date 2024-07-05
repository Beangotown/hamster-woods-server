using System;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Entities.Es;
using Nest;

namespace HamsterWoods.TokenLock;

// public class TokenLockIndex : HamsterWoodsEsEntity<string>, IIndexBuild
// {
//     [Keyword] public override string Id { get; set; }
//     [Keyword] public string CaAddress { get; set; }
//     public int WeekNum { get; set; }
//     public long SumScore { get; set; }
//     [Keyword] public string Symbol { get; set; }
//     public int Decimals { get; set; }
//     public DateTime LockTime { get; set; }
//     public DateTime EstimateUnLockTime { get; set; }
// }