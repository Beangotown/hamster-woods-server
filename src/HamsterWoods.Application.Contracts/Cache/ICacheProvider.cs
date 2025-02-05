using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace HamsterWoods.Cache;

public interface ICacheProvider
{
    public Task SetAsync(string key, string value, TimeSpan? expire);

    public Task<RedisValue> GetAsync(string key);
    Task<T?> Get<T>(string key) where T : class;

    public Task<long> IncreaseAsync(string key, int increase, TimeSpan? expire);
    Task Set<T>(string key, T? value, TimeSpan? expire) where T : class;
}