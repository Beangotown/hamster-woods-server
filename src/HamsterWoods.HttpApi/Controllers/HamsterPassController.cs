using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.Config;
using HamsterWoods.NFT;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("HamsterPass")]
[Route("api/app/hamster-pass/")]
public class HamsterPassController : HamsterWoodsBaseController
{
    private readonly INFTService _nftService;

    public HamsterPassController(INFTService nftService)
    {
        _nftService = nftService;
    }

    [HttpGet]
    [Route("claimable")]
    public async Task<HamsterPassClaimableDto> IsHamsterPassClaimableAsync(HamsterPassInput input)
    {
        return await _nftService.IsHamsterPassClaimableAsync(input);
    }

    [HttpPost]
    [Route("claim")]
    public async Task<HamsterPassDto> ClaimHamsterPassAsync(HamsterPassInput input)
    {
        return await _nftService.ClaimHamsterPassAsync(input);
    }

    /// <summary>
    /// hamster pass list
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("nft-list")]
    public async Task<List<HamsterPassResultDto>> GetNftListAsync(HamsterPassInput input)
    {
        return await _nftService.GetNftListAsync(input);
    }

    /// <summary>
    /// multiple pass change
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("using")]
    public async Task<HamsterPassResultDto> UsingBeanPassAsync(GetHamsterPassInput input)
    {
        return await _nftService.UsingBeanPassAsync(input);
    }

    /// <summary>
    /// user popup
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("popup")]
    public async Task<bool> PopupBeanPassAsync(HamsterPassInput input)
    {
        return await _nftService.PopupBeanPassAsync(input);
    }

    /// <summary>
    /// check whether address has hamster pass
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("check")]
    public async Task<bool> CheckHamsterPassAsync(HamsterPassInput input)
    {
        return await _nftService.CheckHamsterPassAsync(input);
    }

    [HttpGet]
    [Route("config")]
    public Task<ConfigDto> GetConfigAsync()
    {
        return Task.FromResult(new ConfigDto()
            { DailyPlayCountResetTime = 0, ChancePrice = 20, BuyChanceTransactionFee = 0.0035m });
    }

    [HttpGet]
    [Route("asset")]
    public async Task<List<TokenBalanceDto>> GetAssetAsync(HamsterPassInput input)
    {
        return await _nftService.GetAssetAsync(input);
    }

    [HttpGet]
    [Route("price")]
    public async Task<PriceDto> GetPriceAsync()
    {
        return await _nftService.GetPriceAsync();
    }
}