using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.AssetLock.Dtos;

namespace HamsterWoods.AssetLock;

public interface IAssetLockAppService
{
    Task<AssetLockedInfoResultDto> GetLockedInfosAsync(GetAssetLockInfoDto input);
    Task<List<GetUnlockRecordDto>> GetUnlockRecordsAsync(GetAssetLockInfoDto input);
}