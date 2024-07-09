using System.Threading.Tasks;
using HamsterWoods.Rank;

namespace HamsterWoods.Info;

public interface IInfoAppService
{
    Task<CurrentRaceInfoCache> GetCurrentRaceInfoAsync();
}