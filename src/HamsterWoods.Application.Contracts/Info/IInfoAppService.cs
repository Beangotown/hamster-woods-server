using System.Threading.Tasks;
using HamsterWoods.Rank;

namespace HamsterWoods.Info;

public interface IInfoAppService
{
    Task<CurrentRaceInfoCache> GetCurrentRaceInfoAsync();
    Task<object> GetValAsync(string key);
    Task<object> SetValAsync(string key, string val);
}