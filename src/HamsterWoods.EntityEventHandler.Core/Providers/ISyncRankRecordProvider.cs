using System.Threading.Tasks;
using GraphQL;
using HamsterWoods.Common;
using HamsterWoods.EntityEventHandler.Core.Services.Dtos;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.EntityEventHandler.Core.Providers;

public interface ISyncRankRecordProvider
{
    Task<RankRecordsResultDto> GetWeekRankRecordsAsync(int weekNum, int skipCount, int maxResultCount);
}

public class SyncRankRecordProvider : ISyncRankRecordProvider, ISingletonDependency
{
    private readonly IGraphQLHelper _graphQlHelper;

    public SyncRankRecordProvider(IGraphQLHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
    }

    public async Task<RankRecordsResultDto> GetWeekRankRecordsAsync(int weekNum, int skipCount, int maxResultCount)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<GetRankRecordsResultDto>(new GraphQLRequest
        {
            Query = @"
			    query($skipCount:Int!,$maxResultCount:Int!,$weekNum:Int!) {
                    getWeekRankRecords(getRankRecordsDto:{skipCount: $skipCount,maxResultCount:$maxResultCount,weekNum:$weekNum})
                        {
                        rankRecordList {
                            sumScore
                            caAddress
                            symbol
                            decimals
                            weekNum
                            updateTime
                         }
                    }
                }",
            Variables = new
            {
                skipCount,
                maxResultCount,
                weekNum
            }
        });
        return graphQLResponse?.GetWeekRankRecords;
    }
}