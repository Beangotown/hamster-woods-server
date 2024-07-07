using System.Threading.Tasks;
using GraphQL;
using HamsterWoods.AssetLock.Dtos;
using HamsterWoods.Common;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.AssetLock.Provider;

public interface IAssetLockProvider
{
    Task<GetUnlockRecordGqlDto> GetUnlockRecordsAsync(int weekNum, string caAddress, int skipCount, int maxResultCount);
}

public class AssetLockProvider : IAssetLockProvider, ISingletonDependency
{
    private readonly IGraphQLHelper _graphQlHelper;

    public AssetLockProvider(IGraphQLHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
    }
    
    public async Task<GetUnlockRecordGqlDto> GetUnlockRecordsAsync(int weekNum,string caAddress, int skipCount, int maxResultCount)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<GetUnlockRecordGqlResultDto>(new GraphQLRequest
        {
            Query = @"
			    query($skipCount:Int!,$maxResultCount:Int!,$caAddress:String!,$weekNum:Int!) {
                    getUnLockedRecords(getUnLockedRecordsDto:{skipCount: $skipCount,maxResultCount:$maxResultCount,caAddress:$caAddress,weekNum:$weekNum})
                        {
                            unLockRecordList{
                                 id
                                 fromAddress
                                 caAddress
                                 amount
                                 symbol
                                 decimals
                                 weekNum
                                 blockTime
                                 transactionInfo{
                                   transactionId
                                   transactionFee
                                   triggerTime
                                 }
                            }
                            
                        }
                }",
            Variables = new
            {
                skipCount,
                maxResultCount,
                caAddress,
                weekNum
            }
        });
        return graphQLResponse.GetUnLockedRecords;
    }
}