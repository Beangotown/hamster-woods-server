using Volo.Abp.Application.Dtos;

namespace HamsterWoods.AssetLock;

public class GetAssetLockInfoDto: PagedResultRequestDto
{
    public string CaAddress { get; set; }
}