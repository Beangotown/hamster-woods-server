using System.Threading.Tasks;

namespace HamsterWoods.HealthCheck;

public interface IHealthCheckService
{
    Task<bool> ReadyAsync();
    
    Task<bool> CheckCacheAsync();
    
    Task<bool> CheckEsAsync();

    Task<bool> CheckGrainAsync();
}