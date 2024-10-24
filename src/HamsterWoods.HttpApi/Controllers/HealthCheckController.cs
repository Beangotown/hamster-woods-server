using System;
using System.Threading.Tasks;
using HamsterWoods.HealthCheck;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace HamsterWoods.Controllers;

[RemoteService]
[Area("sre")]
[ControllerName("HealthCheck")]
[Route("api/")]
public class HealthCheckController : HamsterWoodsBaseController
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthCheckController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }
    
    [HttpGet]
    [Route("health")]
    public async Task<bool> CheckHealthStatus()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(5));
        return true;
    }
    
    [HttpGet]
    [Route("startup")]
    public async Task<bool> CheckStartupStatus()
    {
        return await _healthCheckService.ReadyAsync();
    }
}