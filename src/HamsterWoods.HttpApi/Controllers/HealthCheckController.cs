using System;
using System.Threading.Tasks;
using HamsterWoods.Commons;
using HamsterWoods.HealthCheck;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("HealthCheck")]
[Route("api/app/")]
public class HealthCheckController : HamsterWoodsBaseController
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthCheckController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }
    
    [HttpGet]
    [Route("health")]
    public async Task<string> CheckHealthStatus()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(5));
        return CommonResult.SuccessCode;
    }
    
    [HttpGet]
    [Route("startup")]
    public async Task<string> CheckStartupStatus()
    {
        if (await _healthCheckService.ReadyAsync())
        {
            return CommonResult.SuccessCode;
        }

        throw new UserFriendlyException("Server is not ready!");
    }
}