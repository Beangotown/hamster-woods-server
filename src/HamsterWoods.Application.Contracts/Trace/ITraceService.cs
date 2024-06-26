using System.Threading.Tasks;

namespace HamsterWoods.Trace;

public interface ITraceService
{
    public Task CreateAsync(GetUserActionDto getUserActionDto);
    
    public Task<StatResultDto> GetStatAsync(GetStatDto getStatDto);
}