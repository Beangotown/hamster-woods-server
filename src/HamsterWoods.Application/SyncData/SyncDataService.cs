using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Cache;
using HamsterWoods.Contract;
using HamsterWoods.TokenLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace HamsterWoods.SyncData;

[RemoteService(false), DisableAuditing]
public class SyncDataService : HamsterWoodsBaseService, ISyncDataService
{
    private readonly INESTRepository<RaceInfoConfigIndex, string> _configRepository;
    private readonly ChainOptions _chainOptions;
    private readonly ICacheProvider _cacheProvider;
    private readonly IContractProvider _contractProvider;
    private readonly ILogger<SyncDataService> _logger;

    public SyncDataService(INESTRepository<RaceInfoConfigIndex, string> configRepository,
        IContractProvider contractProvider, IOptionsMonitor<ChainOptions> chainOptions, ICacheProvider cacheProvider,
        ILogger<SyncDataService> logger)
    {
        _configRepository = configRepository;
        _contractProvider = contractProvider;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _chainOptions = chainOptions.CurrentValue;
    }

    public async Task SyncRaceConfigAsync()
    {
        var raceInfo = await _contractProvider.GetCurrentRaceInfoAsync(_chainOptions.ChainInfos.Keys.First());
        var id = $"{raceInfo.WeekNum}";
        var index = new RaceInfoConfigIndex
        {
            Id = id,
            WeekNum = raceInfo.WeekNum,
            AcornsLockedDays = raceInfo.AcornsLockedDays,
            BeginTime = raceInfo.RaceTimeInfo.BeginTime.ToDateTime(),
            EndTime = raceInfo.RaceTimeInfo.EndTime.ToDateTime(),
            SettleBeginTime = raceInfo.RaceTimeInfo.SettleBeginTime.ToDateTime(),
            SettleEndTime = raceInfo.RaceTimeInfo.SettleEndTime.ToDateTime(),
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        
        var configIndex = await _configRepository.GetAsync(id);
        if (configIndex != null)
        {
            index.CreateTime = configIndex.CreateTime;
        }
        
        _logger.LogInformation("sync race config success, data:{data}", JsonConvert.SerializeObject(index));
        await _configRepository.AddOrUpdateAsync(index);

        //await SaveHis();
    }

    private async Task SaveHis()
    {
        var startDate = "2024-07-05";
        var start = DateTime.Parse(startDate);
        var utcStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        
        // Time = "2024-1-07040705",
        for (int i = 1; i < 6; i++)
        {
            var id = $"{i}";
            var index = new RaceInfoConfigIndex
            {
                Id = id,
                WeekNum = i,
                //AcornsLockedDays = raceInfo.AcornsLockedDays,
                AcornsLockedDays = 1,
                BeginTime = utcStart,
                EndTime = utcStart.AddDays(1),
                SettleBeginTime = utcStart.AddDays(1),
                SettleEndTime = utcStart.AddDays(2),
                CreateTime = DateTime.UtcNow,
                UpdateTime = DateTime.UtcNow
            };

            _logger.LogInformation("sync race config success, data:{data}", JsonConvert.SerializeObject(index));
            await _configRepository.AddOrUpdateAsync(index);
            utcStart = utcStart.AddDays(1);
        }
        
        var index2 = new RaceInfoConfigIndex
        {
            Id = "6",
            WeekNum = 6,
            //AcornsLockedDays = raceInfo.AcornsLockedDays,
            AcornsLockedDays = 1,
            BeginTime = utcStart,
            EndTime = utcStart.AddDays(2),
            SettleBeginTime = utcStart.AddDays(2),
            SettleEndTime = utcStart.AddDays(3),
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };

        _logger.LogInformation("sync race config success, data:{data}", JsonConvert.SerializeObject(index2));
        await _configRepository.AddOrUpdateAsync(index2);
   
     
        
    }
}