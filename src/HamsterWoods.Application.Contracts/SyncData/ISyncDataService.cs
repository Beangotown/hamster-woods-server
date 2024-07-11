using System.Threading.Tasks;

namespace HamsterWoods.SyncData;

public interface ISyncDataService
{
    Task SyncRaceConfigAsync();
}