using System;
using System.Threading.Tasks;
using GraphQL;
using HamsterWoods.Common;
using HamsterWoods.Points.Dtos;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Points.Provider;

public interface IPointsProvider
{
    Task<GetHopCountDto> GetHopCountAsync(DateTime startTime, DateTime endTime, string caAddress);
    Task<GetPurchaseCountDto> GetPurchaseCountAsync(DateTime startTime, DateTime endTime, string caAddress);
}

public class PointsProvider : IPointsProvider, ISingletonDependency
{
    private readonly IGraphQLHelper _graphQlHelper;

    public PointsProvider(IGraphQLHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
    }

    public async Task<GetHopCountDto> GetHopCountAsync(DateTime startTime, DateTime endTime, string caAddress)
    {
        var graphQlResponse = await _graphQlHelper.QueryAsync<GetHopCountDtoGraphDto>(new GraphQLRequest
        {
            Query = @"
			    query($startTime:DateTime,$endTime:DateTime,$address:String!) {
                    getHopCount(requestDto:{startTime:$startTime,endTime:$endTime,address:$address})
                        {
                            hopCount
                        }
                }",
            Variables = new
            {
                startTime,
                endTime,
                address = caAddress
            }
        });
        return graphQlResponse.GetHopCount;
    }

    public async Task<GetPurchaseCountDto> GetPurchaseCountAsync(DateTime startTime, DateTime endTime, string caAddress)
    {
        var graphQlResponse = await _graphQlHelper.QueryAsync<GetPurchaseCountDtoGraphDto>(new GraphQLRequest
        {
            Query = @"
			    query($startTime:DateTime,$endTime:DateTime,$address:String!) {
                    getPurchaseCount(requestDto:{startTime:$startTime,endTime:$endTime,address:$address})
                        {
                            purchaseCount
                        }
                }",
            Variables = new
            {
                startTime,
                endTime,
                address = caAddress
            }
        });
        return graphQlResponse.GetPurchaseCount;
    }
}