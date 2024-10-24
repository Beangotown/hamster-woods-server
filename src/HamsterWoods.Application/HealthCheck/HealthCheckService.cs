using System;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Cache;
using HamsterWoods.Common;
using HamsterWoods.Health;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace HamsterWoods.HealthCheck;

[RemoteService(false), DisableAuditing]
public class HealthCheckService : IHealthCheckService
{
    private const string CheckRedisKey = "CheckRedisKey";
    private const string CheckRedisValue = "CheckRedisValue";
    private const string CheckEsIndexId = "CheckEsIndexId";
    private readonly ICacheProvider _cacheProvider;
    private readonly INESTRepository<HealthCheckIndex, string> _repository;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(ICacheProvider cacheProvider,
        INESTRepository<HealthCheckIndex, string> repository,
        ILogger<HealthCheckService> logger)
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
                    _logger.LogInformation($"Retry {retryCount} encountered {exception.Message}. Waiting {timeSpan} before next retry.");
                });
    }

    public async Task<bool> Ready()
    {
        return await CheckCache() && await CheckEs();
    }

    public async Task<bool> CheckCache()
    {
        await _cacheProvider.SetAsync(CheckRedisKey, CheckRedisKey, TimeSpan.FromSeconds(5));
        var result = await _cacheProvider.GetAsync(CheckRedisKey);
        return result is { IsNullOrEmpty: false, HasValue: true } && CheckRedisValue.Equals(result.ToString());
    }

    public async Task<bool> CheckEs()
    {
        var current = TimeHelper.GetTimeStampInMilliseconds();
        await _repository.AddOrUpdateAsync(new HealthCheckIndex()
        {
            Id = CheckEsIndexId,
            Timestamp = current
        });
        return current == await GetIndexTimestamp();
    }

    private async Task<long> GetIndexTimestamp()
    {
        try
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
        catch (Exception e)
        {
            _logger.LogError(e, "query health check index failed");
        }
        return -1;
    }
}