using System;
using System.Threading.Tasks;
using HamsterWoods.Points;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Hubs;

public class FluxPointsHubProvider : IFluxPointsHubProvider, ISingletonDependency
{
    private readonly ILogger<FluxPointsHubProvider> _logger;
    private readonly IHubContext<FluxPointsHub> _hubContext;

    public FluxPointsHubProvider(ILogger<FluxPointsHubProvider> logger, IHubContext<FluxPointsHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task SendAsync<T>(T data, string connectId, string methodName)
    {
        try
        {
            await _hubContext.Clients.Client(connectId).SendAsync(methodName,
                new HubResponse<T> { Body = data });
            _logger.LogInformation("send success, connectId:{connectId}, methodName:{methodName}", connectId,
                methodName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "send error, connectId:{connectId}, methodName:{methodName}", connectId, methodName);
        }
    }
}