using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HamsterWoods.Commons;
using HamsterWoods.EntityEventHandler.Core.Providers;
using HamsterWoods.Grains.Grain.Unlock;
using HamsterWoods.Options;
using HamsterWoods.Rank;
using HamsterWoods.Unlock;
using HamsterWoods.Unlock.Etos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.EntityEventHandler.Core.Services;

public interface IUnlockAcornsService
{
    Task HandleAsync();
}

public class UnlockAcornsService : IUnlockAcornsService, ISingletonDependency
{
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<UnlockAcornsService> _logger;
    private readonly IUnlockAcornsProvider _unlockAcornsProvider;
    private readonly IClusterClient _clusterClient;
    private readonly IOptionsMonitor<UnlockAcornsOptions> _unlockOptions;
    private readonly IUnlockService _unlockService;
    private readonly IDistributedEventBus _distributedEvent;
    private const string AddressGrainKeyPrefix = "UnlockAddress";

    public UnlockAcornsService(IObjectMapper objectMapper, ILogger<UnlockAcornsService> logger,
        IUnlockAcornsProvider unlockAcornsProvider, IClusterClient clusterClient,
        IOptionsMonitor<UnlockAcornsOptions> unlockOptions, IUnlockService unlockService,
        IDistributedEventBus distributedEvent)
    {
        _objectMapper = objectMapper;
        _logger = logger;
        _unlockAcornsProvider = unlockAcornsProvider;
        _clusterClient = clusterClient;
        _unlockOptions = unlockOptions;
        _unlockService = unlockService;
        _distributedEvent = distributedEvent;
    }

    public async Task HandleAsync()
    {
        await Task.Delay(30000);
        _logger.LogInformation("[UnlockAcorns] UnlockAcorns Start.");
        var raceInfos = await _unlockAcornsProvider.GetRaceConfigAsync();
        if (raceInfos.IsNullOrEmpty())
        {
            _logger.LogWarning("[UnlockAcorns] race info list is empty.");
            return;
        }

        var needUnlockList = raceInfos
            .Where(t => t.SettleBeginTime.AddDays(t.AcornsLockedDays) == DateTime.UtcNow.Date)
            .ToList();

        var raceInfo = needUnlockList?.FirstOrDefault();
        if (raceInfo == null)
        {
            _logger.LogInformation("[UnlockAcorns] no need unlock.");
            return;
        }

        var skip = 0;
        var limit = 500;
        var records = new List<UserWeekRankRecordIndex>();
        var recordList = await _unlockAcornsProvider.GetRecordsAsync(raceInfo.WeekNum, skip, limit);
        while (!recordList.IsNullOrEmpty())
        {
            records.AddRange(recordList);
            skip += limit;
            recordList = await _unlockAcornsProvider.GetRecordsAsync(raceInfo.WeekNum, skip, limit);
        }

        if (records.IsNullOrEmpty())
        {
            _logger.LogWarning("[UnlockAcorns] record list is empty.");
            return;
        }

        _logger.LogInformation("[UnlockAcorns] need send address count: {count}", records.Count);
        await BatchUnlockAsync(raceInfo.WeekNum, records);
        _logger.LogInformation("[UnlockAcorns] UnlockAcorns End.");
    }

    private async Task BatchUnlockAsync(int weekNum, List<UserWeekRankRecordIndex> records)
    {
        var skip = 0;
        var limit = _unlockOptions.CurrentValue.BatchCount;
        var sendRecords = records.Skip(skip).Take(limit).ToList();

        var grainId = IdGenerateHelper.GenerateId(AddressGrainKeyPrefix, weekNum.ToString());
        var addressGrain = _clusterClient.GetGrain<IUnlockAddressGrain>(grainId);

        while (!sendRecords.IsNullOrEmpty())
        {
            skip += limit;
            var addressGrainDto = await addressGrain.SetAddresses(weekNum,
                sendRecords.Select(t => AddressHelper.ToShortAddress(t.CaAddress)).ToList());
            if (!addressGrainDto.Success)
            {
                _logger.LogError("[UnlockAcorns] addressGrainDto fail, message:{message}", addressGrainDto.Message);
                sendRecords = GetRecords(records, skip, limit);
                continue;
            }

            if (addressGrainDto.Data.IsNullOrEmpty())
            {
                _logger.LogWarning("[UnlockAcorns] addressGrainDto return list is empty.");
                sendRecords = GetRecords(records, skip, limit);
                continue;
            }

            var bizId = Guid.NewGuid().ToString();
            var unlockInfoGrain = _clusterClient.GetGrain<IUnlockInfoGrain>(bizId);
            var grainDto = await unlockInfoGrain.SetUnlockInfo(new UnlockInfoGrainDto()
            {
                WeekNum = weekNum,
                BizId = bizId,
                Addresses = addressGrainDto.Data
            });

            if (!grainDto.Success)
            {
                _logger.LogError("[UnlockAcorns] addressGrainDto fail, message:{message}", addressGrainDto.Message);
                sendRecords = GetRecords(records, skip, limit);
                continue;
            }

            await _distributedEvent.PublishAsync(_objectMapper.Map<UnlockInfoGrainDto, UnlockInfoEto>(grainDto.Data));
            await _unlockService.BatchUnlockAsync(grainDto.Data);
            sendRecords = GetRecords(records, skip, limit);
        }
    }

    private List<UserWeekRankRecordIndex> GetRecords(List<UserWeekRankRecordIndex> records, int skip, int limit)
    {
        var sendRecords = records.Skip(skip).Take(limit).ToList();
        _logger.LogInformation(
            "[UnlockAcorns] next handle address count:{count}, currentSkipCount:{currentSkipCount}, limit:{limit}",
            sendRecords.Count, skip, limit);
        
        return sendRecords;
    }
}