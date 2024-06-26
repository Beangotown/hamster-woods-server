using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.NFT;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Route("api/app/hamster-pass/")]
public class HamsterPassController : HamsterWoodsBaseController
{
    private readonly INFTService _nftService;

    [HttpGet]
    [Route("claimable")]
    public async Task<HamsterPassDto> IsBeanPassClaimableAsync(HamsterPassInput input)
    {
        return await _nftService.IsBeanPassClaimableAsync(input);
    }

    [HttpPost]
    [Route("claim")]
    public async Task<HamsterPassDto> ClaimBeanPassAsync(HamsterPassInput input)
    {
        return await _nftService.ClaimBeanPassAsync(input);
    }

    /// <summary>
    /// beanoass list
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("nft-list")]
    public async Task<List<BeanPassResultDto>> GetNftListAsync(HamsterPassInput input)
    {
        return await _nftService.GetNftListAsync(input);
    }

    /// <summary>
    /// mutil beanpass
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("using")]
    public async Task<BeanPassResultDto> UsingBeanPassAsync(GetHamsterPassInput input)
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

    [HttpGet]
    [Route("check")]
    public async Task<bool> CheckBeanPassAsync(HamsterPassInput input)
    {
        return await _nftService.CheckBeanPassAsync(input);
    }
}