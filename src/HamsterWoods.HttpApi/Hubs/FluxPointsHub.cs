using System;
using System.Threading.Tasks;
using HamsterWoods.Points;
using HamsterWoods.Points.Dtos;
using Microsoft.Extensions.Logging;
using Volo.Abp.AspNetCore.SignalR;

namespace HamsterWoods.Hubs;

[HubRoute("api/app/fluxPoints")]
public class FluxPointsHub : AbpHub
{
    private readonly ILogger<FluxPointsHub> _logger;
    private readonly IConnectionProvider _connectionProvider;
    private readonly IPointHubService _pointHubService;

    public FluxPointsHub(ILogger<FluxPointsHub> logger, IConnectionProvider connectionProvider, IPointHubService pointHubService)
    {
        _logger = logger;
        _connectionProvider = connectionProvider;
        _pointHubService = pointHubService;
    }

    public async Task Connect(string clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            return;
        }

        //await _hubService.RegisterClientAsync(clientId, Context.ConnectionId);
        _connectionProvider.Add(clientId, Context.ConnectionId);
        _logger.LogInformation("clientId={clientId}, connectionId={connectionId} connect", clientId,
            Context.ConnectionId);
    }

    public async Task PointsList(PointsListRequestDto request)
    {
        await _pointHubService.RequestPointsList(request);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectionProvider.Remove(Context.ConnectionId);
        _logger.LogInformation("connectionId={connectionId} disconnected!!!", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}