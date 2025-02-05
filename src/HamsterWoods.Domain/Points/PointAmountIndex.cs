using AElf.Indexing.Elasticsearch;
using HamsterWoods.Entities.Es;
using Nest;

namespace HamsterWoods.Points;

public class PointAmountIndex : HamsterWoodsEsEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string Address { get; set; }
    [Keyword] public string ConnectionId { get; set; }
    public long FirstSymbolAmount { get; set; }
    public long SecondSymbolAmount { get; set; }
    public long ThirdSymbolAmount { get; set; }
    public long FourSymbolAmount { get; set; }
    public long FiveSymbolAmount { get; set; }
    public long SixSymbolAmount { get; set; }
    public long SevenSymbolAmount { get; set; }
    public long EightSymbolAmount { get; set; }
    public long NineSymbolAmount { get; set; }
    public long TenSymbolAmount { get; set; }
    public long ElevenSymbolAmount { get; set; }
    public long TwelveSymbolAmount { get; set; }
}