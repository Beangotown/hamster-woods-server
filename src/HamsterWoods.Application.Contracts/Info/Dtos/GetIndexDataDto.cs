using Nest;
using Volo.Abp.Application.Dtos;

namespace HamsterWoods.Info.Dtos;

public class GetIndexDataDto : PagedResultRequestDto
{
    public string Filter { get; set; }
    public string Sort { get; set; }
}

public class SortType
{
    public string SortField { get; set; }
    public SortOrder SortOrder { get; set; }
}