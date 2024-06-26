using System.Collections.Generic;
using System.Threading.Tasks;

namespace HamsterWoods.NFT;

public interface INFTService
{
    public Task<HamsterPassDto> ClaimHamsterPassAsync(HamsterPassInput input);

    public Task<HamsterPassClaimableDto> IsHamsterPassClaimableAsync(HamsterPassInput input);

    public Task<List<HamsterPassResultDto>> GetNftListAsync(HamsterPassInput input);
    
    public Task<HamsterPassResultDto> UsingBeanPassAsync(GetHamsterPassInput input);
    
    Task<bool> PopupBeanPassAsync(HamsterPassInput input);

    Task<bool> CheckHamsterPassAsync(HamsterPassInput input);
}