using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HamsterWoods.Hubs;
using HamsterWoods.Points.Dtos;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Points;

public class PointHubService : IPointHubService, ISingletonDependency
{
    private readonly ILogger<PointHubService> _logger;
    private readonly IFluxPointsHubProvider _pointsHubProvider;
    private readonly IConnectionProvider _connectionProvider;

    public PointHubService(IFluxPointsHubProvider pointsHubProvider, ILogger<PointHubService> logger,
        IConnectionProvider connectionProvider)
    {
        _pointsHubProvider = pointsHubProvider;
        _logger = logger;
        _connectionProvider = connectionProvider;
    }

    public async Task RequestPointsList(PointsListRequestDto request)
    {
        var cts = new CancellationTokenSource(10000);
        while (!cts.IsCancellationRequested)
        {
            try
            {
                // stop while disconnected
                var connectionInfo = _connectionProvider.GetConnectionByClientId(request.TargetClientId);
                if (connectionInfo == null || connectionInfo.ConnectionId.IsNullOrEmpty())
                {
                    _logger.LogWarning("connection disconnected, clientId:{clientId}", request.TargetClientId);
                    break;
                }

                //query from proxi 
                var dto = new List<FluxPointsDto>();
                dto.Add(new FluxPointsDto()
                {
                    Behavior = "Participate Daily HOP tasks",
                    PointAmount = 29,
                    PointName = "ACORNS Point-2"
                });
                await _pointsHubProvider.SendAsync<List<FluxPointsDto>>(dto, connectionInfo.ConnectionId, "pointsListChange");
                // _logger.LogInformation("Get third-part order {OrderId} {CallbackMethod}  success",
                //     orderId, callbackMethod);
                await Task.Delay(3000);
            }
            catch (OperationCanceledException oce)
            {
                _logger.LogError(oce,
                    "Timed out waiting get flux points, clientId:{clientId}", request.TargetClientId);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "An exception occurred during query flux points, clientId:{clientId}", request.TargetClientId);
                break;
            }
        }

        cts.Cancel();
    }
}