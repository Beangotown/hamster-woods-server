using Volo.Abp.Application.Dtos;

namespace HamsterWoods.Rank;

public class GetRankDto : PagedResultRequestDto
{
    public string CaAddress { get; set; }
}