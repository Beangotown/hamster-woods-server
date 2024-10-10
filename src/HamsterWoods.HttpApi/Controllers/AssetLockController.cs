using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using HamsterWoods.AssetLock;
using HamsterWoods.AssetLock.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("AssetLock")]
[Route("api/app/lock/")]
public class AssetLockController : HamsterWoodsBaseController
{
    private readonly IAssetLockAppService _assetLockAppService;

    public AssetLockController(IAssetLockAppService assetLockAppService)
    {
        _assetLockAppService = assetLockAppService;
    }

    [HttpGet]
    [Route("locked-infos")]
    public async Task<AssetLockedInfoResultDto> GetLockedInfosAsync(GetAssetLockInfoDto input)
    {
        return await _assetLockAppService.GetLockedInfosAsync(input);
    }

    [HttpGet]
    [Route("unlock-records")]
    public async Task<List<GetUnlockRecordDto>> GetUnlockRecordsAsync(GetAssetLockInfoDto input)
    {
        return await _assetLockAppService.GetUnlockRecordsAsync(input);
    }
}