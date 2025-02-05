using System;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Entities.Es;
using Nest;

namespace HamsterWoods.Trace;

public class UserActionIndex : HamsterWoodsEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string CaAddress { get; set; }
    [Keyword] public string CaHash { get; set; }
    [Keyword] public string ChainId { get; set; }
    public UserActionType ActionType { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum UserActionType
{
    Register,
    Login
}