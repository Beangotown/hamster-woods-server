using System.Threading.Tasks;
using HamsterWoods.Hubs;

namespace HamsterWoods.Points;

public interface IFluxPointsHubProvider
{
    Task SendAsync<T>(T data, string connectId, string methodName);
}