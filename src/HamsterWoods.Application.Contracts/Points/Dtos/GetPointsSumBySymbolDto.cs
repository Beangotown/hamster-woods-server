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
    public string FirstSymbolAmount { get; set; }
    public string SecondSymbolAmount { get; set; }
    public string ThirdSymbolAmount { get; set; }
}