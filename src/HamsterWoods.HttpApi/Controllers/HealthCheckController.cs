using System;
using System.Threading.Tasks;
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
        return "200";
    }
    
    [HttpGet]
    [Route("startup")]
    public async Task<string> CheckStartupStatus()
    {
        return await _healthCheckService.ReadyAsync() ? "200" : "500";
    }
}