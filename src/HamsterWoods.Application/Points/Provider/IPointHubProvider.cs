using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using HamsterWoods.Options;
using HamsterWoods.Points.Dtos;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Points.Provider;

public interface IPointHubProvider
{
    Task<GetPointsSumBySymbolResultDto> GetPointsSumBySymbolAsync(List<string> addressList, string dappId,
        int skipCount,
        int maxResultCount);

    Task<PointAmountIndex> GetPointAmountAsync(string address);
}

public class PointHubProvider : IPointHubProvider, ISingletonDependency
{
    private readonly INESTRepository<PointAmountIndex, string> _repository;
    private readonly IGraphQLClient _graphQlClient;

    public PointHubProvider(INESTRepository<PointAmountIndex, string> repository,
        IOptionsSnapshot<FluxPointsOptions> options)
    {
        _repository = repository;
        _graphQlClient =
            new GraphQLHttpClient(
                options.Value.Graphql,
                new NewtonsoftJsonSerializer());
    }

    public async Task<PointAmountIndex> GetPointAmountAsync(string address)
    {
        return await _repository.GetAsync(address);
    }

    public async Task<GetPointsSumBySymbolResultDto> GetPointsSumBySymbolAsync(List<string> addressList, string dappId,
        int skipCount,
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
                    role
                    firstSymbolAmount
                    secondSymbolAmount
                    thirdSymbolAmount
                    fourSymbolAmount
                    fiveSymbolAmount
                    sixSymbolAmount
                    sevenSymbolAmount
                    eightSymbolAmount
                    nineSymbolAmount
                    tenSymbolAmount
                    elevenSymbolAmount
                    twelveSymbolAmount
                 }
                }
            }",
            Variables = new
            {
                addressList,
                dappName = dappId,
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