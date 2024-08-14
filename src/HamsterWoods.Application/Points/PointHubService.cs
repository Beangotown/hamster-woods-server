using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using HamsterWoods.Commons;
using HamsterWoods.Hubs;
using HamsterWoods.Options;
using HamsterWoods.Points.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Points;

public class PointHubService : IPointHubService, ISingletonDependency
{
    private readonly ILogger<PointHubService> _logger;
    private readonly IFluxPointsHubProvider _pointsHubProvider;
    private readonly IConnectionProvider _connectionProvider;
    private readonly IGraphQLClient _graphQlClient;
    private readonly IOptionsMonitor<FluxPointsOptions> _options;

    public PointHubService(IFluxPointsHubProvider pointsHubProvider, ILogger<PointHubService> logger,
        IConnectionProvider connectionProvider, IOptionsMonitor<FluxPointsOptions> options)
    {
        _pointsHubProvider = pointsHubProvider;
        _logger = logger;
        _connectionProvider = connectionProvider;
        _options = options;
        _graphQlClient =
            new GraphQLHttpClient(
                options.CurrentValue.Graphql,
                new NewtonsoftJsonSerializer());
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

                var fluxPointsList = await GetFluxPointsAsync(request.CaAddress);
                if (fluxPointsList.IsNullOrEmpty())
                {
                    await Task.Delay(_options.CurrentValue.Period);
                    continue;
                }

                await _pointsHubProvider.SendAsync(fluxPointsList, connectionInfo.ConnectionId,
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

    public async Task<List<FluxPointsDto>> GetFluxPointsAsync(string address)
    {
        var fluxPointsList = new List<FluxPointsDto>();
        var result =
            await GetPointsSumBySymbolAsync(
                new List<string> { AddressHelper.ToShortAddress(address) }, 0, 20);

        var pointsInfo = result?.Data?.FirstOrDefault();
        if (pointsInfo == null)
        {
            return fluxPointsList;
        }
        
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

        return fluxPointsList;
    }

    // todo: dappName how to set.
    private async Task<GetPointsSumBySymbolResultDto> GetPointsSumBySymbolAsync(List<string> addressList, int skipCount,
        int maxResultCount)
    {
        var graphQLResponse = await QueryAsync<GetPointsSumBySymbolResultGqlDto>(new GraphQLRequest
        {
            Query = @"
            query($addressList:[String!]!,$dappName:String!,$skipCount:Int!,$maxResultCount:Int!) {
              getPointsSumBySymbol(input:{addressList:$addressList,dappName:$dappName,skipCount:$skipCount,maxResultCount:$maxResultCount}){
                 totalRecordCount
                 data {
                  address
                  domain
                  firstSymbolAmount
                  secondSymbolAmount
                  thirdSymbolAmount
                  fourSymbolAmount
                 }
                }
            }",
            Variables = new
            {
                addressList,
                dappName = "",
                skipCount,
                maxResultCount
            }
        });
        return graphQLResponse?.GetPointsSumBySymbol;
    }

    private async Task<T> QueryAsync<T>(GraphQLRequest request)
    {
        var graphQlResponse = await _graphQlClient.SendQueryAsync<T>(request);
        if (graphQlResponse.Errors is not { Length: > 0 }) return graphQlResponse.Data;
        return default;
    }
}