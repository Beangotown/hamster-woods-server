using System.Threading.Tasks;
using HamsterWoods.Cache;
using HamsterWoods.Rank;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Info;

public class InfoAppService : IInfoAppService, ISingletonDependency
{
    private readonly IRankProvider _rankProvider;
    private readonly ICacheProvider _cacheProvider;

    public InfoAppService(IRankProvider rankProvider, ICacheProvider cacheProvider)
    {
        _rankProvider = rankProvider;
        _cacheProvider = cacheProvider;
    }

    public async Task<CurrentRaceInfoCache> GetCurrentRaceInfoAsync()
    {
        var weekInfo = await _rankProvider.GetCurrentRaceInfoAsync();
        return weekInfo;
    }

    public async Task<object> GetValAsync(string key)
    {
       var val= await _cacheProvider.GetAsync(key);
       if (!val.IsNull) return val.ToString();
       return val;
    }

    public async Task<object> SetValAsync(string key, string val)
    {
        await _cacheProvider.SetAsync(key, val, null);
        return await GetValAsync(key);
    }
}