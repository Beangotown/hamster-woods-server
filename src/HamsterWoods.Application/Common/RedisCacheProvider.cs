using System;
using System.Threading.Tasks;
using HamsterWoods.Cache;
using StackExchange.Redis;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Common;

public class RedisCacheProvider : ICacheProvider, ISingletonDependency
{
    private const string RedisPrefix = "hamsterwoods:";
    private readonly IDatabase _database;

    public RedisCacheProvider(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }


    public async Task SetAsync(string key, string value, TimeSpan? expire)
    {
        await _database.StringSetAsync(GetKey(key), value);
        if (expire != null) _database.KeyExpire(GetKey(key), expire);
    }

    public async Task<RedisValue> GetAsync(string key)
    {
        return await _database.StringGetAsync(GetKey(key));
    }

    public async Task<long> IncreaseAsync(string key, int increase, TimeSpan? expire)
    {
        var count = await _database.StringIncrementAsync(GetKey(key), increase);
        if (expire != null) _database.KeyExpire(GetKey(key), expire);

        return count;
    }

    private string GetKey(string key)
    {
        return RedisPrefix + key;
    }
}