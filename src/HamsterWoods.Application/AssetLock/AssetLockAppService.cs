using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.AssetLock.Dtos;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace HamsterWoods.AssetLock;

[RemoteService(false), DisableAuditing]
public class AssetLockAppService : HamsterWoodsBaseService, IAssetLockAppService
{
    private readonly ILogger<AssetLockAppService> _logger;

    public AssetLockAppService(ILogger<AssetLockAppService> logger)
    {
        _logger = logger;
    }

    public Task<List<AssetLockedInfoDto>> GetLockedInfosAsync(GetAssetLockInfoDto input)
    {
        return Task.FromResult(new List<AssetLockedInfoDto>());
    }

    public Task<List<GetUnlockRecordDto>> GetUnlockRecordsAsync(GetAssetLockInfoDto input)
    {
        return Task.FromResult(new List<GetUnlockRecordDto>());
    }
}