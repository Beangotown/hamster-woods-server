using System.Collections.Generic;
using System.Threading.Tasks;

namespace HamsterWoods.NFT;

public interface INFTService
{
    public Task<HamsterPassDto> ClaimBeanPassAsync(HamsterPassInput input);


    public Task<HamsterPassDto> IsBeanPassClaimableAsync(HamsterPassInput input);

    public Task<List<BeanPassResultDto>> GetNftListAsync(HamsterPassInput input);
    
    public Task<BeanPassResultDto> UsingBeanPassAsync(GetHamsterPassInput input);
    
    Task<bool> PopupBeanPassAsync(HamsterPassInput input);

    Task<bool> CheckBeanPassAsync(HamsterPassInput input);
}