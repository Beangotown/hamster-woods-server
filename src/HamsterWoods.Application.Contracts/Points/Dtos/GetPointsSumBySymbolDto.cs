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
    public long FirstSymbolAmount { get; set; }
    public long SecondSymbolAmount { get; set; }
    public long ThirdSymbolAmount { get; set; }
}