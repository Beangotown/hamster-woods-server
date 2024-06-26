using System;
using System.Collections.Generic;
using AElf.Indexing.Elasticsearch;
using Nest;

namespace HamsterWoods.Entities.Es;

public class SystemMessageInfoIndex: HamsterWoodsEsEntity<Guid>, IIndexBuild
{
    [Keyword] public override Guid Id { get; set; }
    [Keyword] public string AppId { get; set; }
    [Keyword] public string MessageType { get; set; }
    [Keyword] public string Content { get; set; }
    [Keyword] public string Title { get; set; }
    [Keyword] public string Icon { get; set; }
    [Keyword] public string TargetDeviceType { get; set; }
    public List<string> Topics { get; set; }
}