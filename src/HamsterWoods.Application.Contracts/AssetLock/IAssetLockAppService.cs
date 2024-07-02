using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.AssetLock.Dtos;

namespace HamsterWoods.AssetLock;

public interface IAssetLockAppService
{
    Task<List<AssetLockedInfoDto>> GetLockedInfosAsync(GetAssetLockInfoDto input);
    Task<List<GetUnlockRecordDto>> GetUnlockRecordsAsync(GetAssetLockInfoDto input);
}