using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.NFT;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Route("api/app/bean-pass/")]
public class HamsterPassController:HamsterWoodsBaseController
{
    private readonly INFTService _nftService;
    
    [HttpGet]
    [Route("claimable")]
    public async Task<BeanPassDto> IsBeanPassClaimableAsync(BeanPassInput input)
    {
        return await _nftService.IsBeanPassClaimableAsync(input);
    }

    [HttpPost]
    [Route("claim")]
    public async Task<BeanPassDto> ClaimBeanPassAsync(BeanPassInput input)
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
    public async Task<List<BeanPassResultDto>> GetNftListAsync(BeanPassInput input)
    {
        return await _nftService.GetNftListAsync(input);
    }
    
    /// <summary>
    /// 多个beanpass 可以切
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("using")]
    public async Task<BeanPassResultDto> UsingBeanPassAsync(GetBeanPassInput input)
    {
        return await _nftService.UsingBeanPassAsync(input);
    }
    
    /// <summary>
    /// 用户弹窗，新用户是否弹过窗
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("popup")]
    public async Task<bool> PopupBeanPassAsync(BeanPassInput input)
    {
        return await _nftService.PopupBeanPassAsync(input);
    }
    
    [HttpGet]
    [Route("check")]
    public async Task<bool> CheckBeanPassAsync(BeanPassInput input)
    {
        return await _nftService.CheckBeanPassAsync(input);
    }
}