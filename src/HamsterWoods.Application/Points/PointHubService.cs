using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using HamsterWoods.Commons;
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
    private readonly IGraphQLClient _graphQlClient;

    public PointHubService(IFluxPointsHubProvider pointsHubProvider, ILogger<PointHubService> logger,
        IConnectionProvider connectionProvider)
    {
        _pointsHubProvider = pointsHubProvider;
        _logger = logger;
        _connectionProvider = connectionProvider;
        _graphQlClient =
            new GraphQLHttpClient(
                "https://test-indexer.pixiepoints.io/AElfIndexer_Points/PointsIndexerPluginSchema/graphql",
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

                //query from proxi 
                // var result =
                //     await GetPointsSumBySymbolAsync(
                //         new List<string> { AddressHelper.ToShortAddress(request.CaAddress) }, 0, 20);
                // if (result.Data.IsNullOrEmpty())
                // {
                //     continue;
                // }

                var dto = new List<FluxPointsDto>();
                dto.Add(new FluxPointsDto()
                {
                    Behavior = "Participate Daily HOP tasks",
                    PointAmount = 29,
                    PointName = "ACORNS Point-2"
                });
                await _pointsHubProvider.SendAsync<List<FluxPointsDto>>(dto, connectionInfo.ConnectionId,
                    "pointsListChange");
                // _logger.LogInformation("Get third-part order {OrderId} {CallbackMethod}  success",
                //     orderId, callbackMethod);
                await Task.Delay(3000);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "An exception occurred during query flux points, clientId:{clientId}", request.TargetClientId);
                break;
            }
        }
    }

    public async Task<GetPointsSumBySymbolResultDto> GetPointsSumBySymbolAsync(List<string> addressList, int skipCount,
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

    public async Task<T> QueryAsync<T>(GraphQLRequest request)
    {
        var graphQlResponse = await _graphQlClient.SendQueryAsync<T>(request);
        if (graphQlResponse.Errors is not { Length: > 0 }) return graphQlResponse.Data;
        return default;
    }
}