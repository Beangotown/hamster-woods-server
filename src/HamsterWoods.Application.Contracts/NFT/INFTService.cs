using System.Collections.Generic;
using System.Threading.Tasks;

namespace HamsterWoods.NFT;

public interface INFTService
{
    public Task<BeanPassDto> ClaimBeanPassAsync(BeanPassInput input);


    public Task<BeanPassDto> IsBeanPassClaimableAsync(BeanPassInput input);

    public Task<List<BeanPassResultDto>> GetNftListAsync(BeanPassInput input);
    
    public Task<BeanPassResultDto> UsingBeanPassAsync(GetBeanPassInput input);
    
    Task<bool> PopupBeanPassAsync(BeanPassInput input);

    Task<bool> CheckBeanPassAsync(BeanPassInput input);
}