using System.Threading.Tasks;
using HamsterWoods.Rank;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Info;

public class InfoAppService : IInfoAppService, ISingletonDependency
{
    private readonly IRankProvider _rankProvider;

    public InfoAppService(IRankProvider rankProvider)
    {
        _rankProvider = rankProvider;
    }

    public async Task<CurrentRaceInfoCache> GetCurrentRaceInfoAsync()
    {
        var weekInfo = await _rankProvider.GetCurrentRaceInfoAsync();
        return weekInfo;
    }
}