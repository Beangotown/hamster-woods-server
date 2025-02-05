using System.Threading.Tasks;
using HamsterWoods.SyncData.Dtos;

namespace HamsterWoods.SyncData;

public interface ISyncDataService
{
    Task<CurrentRaceDto> SyncRaceConfigAsync();
}