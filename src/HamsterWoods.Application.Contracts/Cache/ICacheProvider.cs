using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace HamsterWoods.Cache;

public interface ICacheProvider
{
    public Task SetAsync(string key, string value, TimeSpan? expire);

    public Task<RedisValue> GetAsync(string key);

    public Task<long> IncreaseAsync(string key, int increase, TimeSpan? expire);
}