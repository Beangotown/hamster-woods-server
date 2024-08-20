using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HamsterWoods.Commons;
using HamsterWoods.Enums;
using HamsterWoods.Hubs;
using HamsterWoods.Options;
using HamsterWoods.Points.Dtos;
using HamsterWoods.Points.Etos;
using HamsterWoods.Points.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Points;

public class PointHubService : IPointHubService, ISingletonDependency
{
    private readonly ILogger<PointHubService> _logger;
    private readonly IFluxPointsHubProvider _pointsHubProvider;
    private readonly IConnectionProvider _connectionProvider;
    private readonly IOptionsMonitor<FluxPointsOptions> _options;
    private readonly IPointHubProvider _pointHubProvider;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IObjectMapper _mapper;

    public PointHubService(IFluxPointsHubProvider pointsHubProvider, ILogger<PointHubService> logger,
        IConnectionProvider connectionProvider, IOptionsMonitor<FluxPointsOptions> options,
        IPointHubProvider pointHubProvider, IDistributedEventBus distributedEventBus, IObjectMapper mapper)
    {
        _pointsHubProvider = pointsHubProvider;
        _logger = logger;
        _connectionProvider = connectionProvider;
        _options = options;
        _pointHubProvider = pointHubProvider;
        _distributedEventBus = distributedEventBus;
        _mapper = mapper;
    }

    public async Task RequestPointsList(PointsListRequestDto request)
    {
        //var cts = new CancellationTokenSource(10000);
        while (true)
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

                var fluxPointResultDto = await GetFluxPointsAsync(request.CaAddress, connectionInfo.ConnectionId);
                if (fluxPointResultDto.FluxPointsList.IsNullOrEmpty() || !fluxPointResultDto.IsChange)
                {
                    await Task.Delay(_options.CurrentValue.Period);
                    continue;
                }

                await _pointsHubProvider.SendAsync(fluxPointResultDto.FluxPointsList, connectionInfo.ConnectionId,
                    "pointsListChange");

                _logger.LogInformation("Get flux points success, address:{address}", request.CaAddress);
                await Task.Delay(_options.CurrentValue.Period);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "An exception occurred during query flux points, clientId:{clientId}, address:{address}",
                    request.TargetClientId, request.CaAddress);
                break;
            }
        }
    }

    public async Task<FluxPointResultDto> GetFluxPointsAsync(string address, string connectionId)
    {
        var resultDto = new FluxPointResultDto();
        var fluxPointsList = new List<FluxPointsDto>();
        var result =
            await _pointHubProvider.GetPointsSumBySymbolAsync(
                new List<string> { AddressHelper.ToShortAddress(address) }, _options.CurrentValue.DappId, 0, 100);

        var pointsInfo = result?.Data?.FirstOrDefault(t => t.Role == PointRoleType.USER.ToString());

        pointsInfo ??= new GetPointsSumBySymbolDto();
        var secondInfo = _options.CurrentValue.PointsInfos.GetOrDefault(nameof(pointsInfo.SecondSymbolAmount));
        fluxPointsList.Add(new FluxPointsDto()
        {
            Behavior = secondInfo?.Behavior,
            PointAmount = (int)(pointsInfo.SecondSymbolAmount / Math.Pow(10, 8)),
            PointName = secondInfo?.PointName
        });

        var thirdInfo = _options.CurrentValue.PointsInfos.GetOrDefault(nameof(pointsInfo.ThirdSymbolAmount));
        fluxPointsList.Add(new FluxPointsDto()
        {
            Behavior = thirdInfo.Behavior,
            PointAmount = (int)(pointsInfo.ThirdSymbolAmount / Math.Pow(10, 8)),
            PointName = thirdInfo?.PointName
        });
        var compare = await CompareAsync(pointsInfo, AddressHelper.ToShortAddress(address), connectionId);

        if (!compare)
        {
            await SaveChangeAsync(pointsInfo, connectionId);
        }

        resultDto.FluxPointsList = fluxPointsList;
        resultDto.IsChange = !compare;
        return resultDto;
    }

    private async Task SaveChangeAsync(GetPointsSumBySymbolDto symbolDto, string connectionId)
    {
        var pointAmountEto = _mapper.Map<GetPointsSumBySymbolDto, PointAmountEto>(symbolDto);
        pointAmountEto.ConnectionId = connectionId;
        await _distributedEventBus.PublishAsync(pointAmountEto, false, false);
    }

    private async Task<bool> CompareAsync(GetPointsSumBySymbolDto symbolDto, string address, string connectionId)
    {
        if (connectionId.IsNullOrEmpty())
        {
            return true;
        }

        var amountInfo = await _pointHubProvider.GetPointAmountAsync(address);
        if (amountInfo == null || amountInfo.ConnectionId != connectionId)
        {
            return false;
        }

        if (amountInfo.SecondSymbolAmount == symbolDto.SecondSymbolAmount &&
            amountInfo.ThirdSymbolAmount == symbolDto.ThirdSymbolAmount
            // amountInfo.FourSymbolAmount == symbolDto.FourSymbolAmount&&
            // amountInfo.FiveSymbolAmount == symbolDto.FiveSymbolAmount&&
            // amountInfo.SixSymbolAmount == symbolDto.SixSymbolAmount&&
            // amountInfo.SevenSymbolAmount == symbolDto.SevenSymbolAmount&&
            // amountInfo.EightSymbolAmount == symbolDto.EightSymbolAmount&&
            // amountInfo.NineSymbolAmount == symbolDto.NineSymbolAmount&&
            // amountInfo.TenSymbolAmount == symbolDto.TenSymbolAmount&&
            // amountInfo.ElevenSymbolAmount == symbolDto.ElevenSymbolAmount&&
            // amountInfo.TwelveSymbolAmount == symbolDto.TwelveSymbolAmount
           )
        {
            return true;
        }

        return false;
    }
}