using System.Collections.Generic;

namespace HamsterWoods.Points.Dtos;

public class GetPointsSumBySymbolResultGqlDto
{
    public GetPointsSumBySymbolResultDto GetPointsSumBySymbol { get; set; }
}

public class GetPointsSumBySymbolResultDto
{
    public List<GetPointsSumBySymbolDto> Data { get; set; }
    public long TotalRecordCount { get; set; }
}

public class GetPointsSumBySymbolDto
{
    public string Address { get; set; }
    public string Domain { get; set; }
    public string Role { get; set; }
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