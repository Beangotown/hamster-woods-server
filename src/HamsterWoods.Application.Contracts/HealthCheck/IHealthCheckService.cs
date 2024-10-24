using System.Threading.Tasks;

namespace HamsterWoods.HealthCheck;

public interface IHealthCheckService
{
    Task<bool> Ready();
    
    Task<bool> CheckCache();
    
    Task<bool> CheckEs();
}