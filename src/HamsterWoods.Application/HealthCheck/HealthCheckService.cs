using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Cache;
using HamsterWoods.Common;
using HamsterWoods.Grains.Grain.HealthCheck;
using HamsterWoods.Health;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans;
using Polly;
using Polly.Retry;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace HamsterWoods.HealthCheck;

[RemoteService(false), DisableAuditing]
public class HealthCheckService : HamsterWoodsBaseService, IHealthCheckService
{
    private const string CheckRedisKey = "CheckRedisKey";
    private const string CheckRedisValue = "CheckRedisValue";
    private const string CheckEsIndexId = "CheckEsIndexId";
    private const string DefaultGrainId = "Grain-HealthCheck";
    private readonly ICacheProvider _cacheProvider;
    private readonly INESTRepository<HealthCheckIndex, string> _repository;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IClusterClient _clusterClient;

    public HealthCheckService(ICacheProvider cacheProvider,
        INESTRepository<HealthCheckIndex, string> repository,
        ILogger<HealthCheckService> logger,
        IClusterClient clusterClient)
    {
        _cacheProvider = cacheProvider;
        _repository = repository;
        _logger = logger;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 4, 
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(500),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.Write($"Retry {retryCount} encountered {exception.Message}. Waiting {timeSpan} before next retry.");
                });
        _clusterClient = clusterClient;
    }

    public async Task<bool> ReadyAsync()
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var result = await CheckCacheAsync() && await CheckEsAsync() && await CheckGrainAsync();
        stopWatch.Stop();
        _logger.LogInformation("HealthCheckService#ReadyAsync cost:{0}ms", stopWatch.ElapsedMilliseconds);
        return result;
    }

    public async Task<bool> CheckCacheAsync()
    {
        await _cacheProvider.SetAsync(CheckRedisKey, CheckRedisValue, TimeSpan.FromSeconds(10));
        var result = await _cacheProvider.GetAsync(CheckRedisKey);
        _logger.LogInformation("CheckCacheAsync GetAsync result {0}", JsonConvert.SerializeObject(result));
        return result is { IsNullOrEmpty: false, HasValue: true } && CheckRedisValue.Equals(result.ToString());
    }

    public async Task<bool> CheckEsAsync()
    {
        var current = TimeHelper.GetTimeStampInMilliseconds();
        await _repository.AddOrUpdateAsync(new HealthCheckIndex()
        {
            Id = CheckEsIndexId,
            Timestamp = current
        });
        var index = await _repository.GetAsync(CheckEsIndexId);
        _logger.LogInformation("HealthCheckIndex: {0}", JsonConvert.SerializeObject(index));
        return current == await GetIndexTimestamp();
    }

    private async Task<long> GetIndexTimestamp()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var index = await _repository.GetAsync(CheckEsIndexId);
            if (index == null)
            {
                throw new UserFriendlyException("query health check index failed, retrying...");
            }
            return index.Timestamp;
        });
    }

    public async Task<bool> CheckGrainAsync()
    {
        var grain = _clusterClient.GetGrain<IHealthCheckGrain>(DefaultGrainId);
        var current = TimeHelper.GetTimeStampInMilliseconds();
        var result = await grain.CreateOrUpdateHealthCheckData(new HealthCheckGrainDto()
        {
            Id = DefaultGrainId,
            Timestamp = current
        });
        _logger.LogInformation("CheckGrainAsync#CreateOrUpdateHealthCheckData {0}", JsonConvert.SerializeObject(result));
        if (!result.Success)
        {
            return false;
        }

        var getResult = await grain.GetHealthCheckData();
        _logger.LogInformation("CheckGrainAsync#GetHealthCheckData {0}", JsonConvert.SerializeObject(getResult));
        return getResult.Success && getResult.Data.Timestamp.Equals(current);
    }
}