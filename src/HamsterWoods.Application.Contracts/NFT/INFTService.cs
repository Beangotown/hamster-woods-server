using System.Collections.Generic;
using System.Threading.Tasks;

namespace HamsterWoods.NFT;

public interface INFTService
{
    public Task<HamsterPassDto> ClaimHamsterPassAsync(HamsterPassInput input);

    public Task<HamsterPassClaimableDto> IsHamsterPassClaimableAsync(HamsterPassInput input);

    public Task<List<HamsterPassResultDto>> GetNftListAsync(HamsterPassInput input);

    Task<bool> CheckHamsterPassAsync(HamsterPassInput input);
    Task<List<TokenBalanceDto>> GetAssetAsync(HamsterPassInput input);
    Task<PriceDto> GetPriceAsync();
}