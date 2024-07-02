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
        return Task.FromResult(new List<AssetLockedInfoDto>()
        {
            new AssetLockedInfoDto()
            {
                LockedTime = "2024-06-28",
                UnLockTime = "2024-07-24",
                Symbol = "ACORNS",
                Decimals = 8,
                Amount = 100
            }
        });
    }

    public Task<List<GetUnlockRecordDto>> GetUnlockRecordsAsync(GetAssetLockInfoDto input)
    {
        return Task.FromResult(new List<GetUnlockRecordDto>() {
            new GetUnlockRecordDto()
            {
                UnLockTime = "2024-07-24",
                Symbol = "ACORNS",
                Decimals = 8,
                Amount = 100,
                TransactionId="685fa94f58d5176438b678ebf317fc23fb6539adc66127c6221b7a18a4a20364"
            }
        });
    }
}