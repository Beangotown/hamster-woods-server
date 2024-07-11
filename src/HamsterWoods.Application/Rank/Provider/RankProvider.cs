using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using GraphQL;
using HamsterWoods.Cache;
using HamsterWoods.Common;
using HamsterWoods.Contract;
using HamsterWoods.TokenLock;
using HamsterWoods.Trace;
using Microsoft.Extensions.Options;
using Nest;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Rank.Provider;

public class RankProvider : IRankProvider, ISingletonDependency
{
    private readonly IGraphQLHelper _graphQlHelper;
    private readonly ChainOptions _chainOptions;
    private readonly ICacheProvider _cacheProvider;
    private readonly IContractProvider _contractProvider;
    private readonly INESTRepository<RaceInfoConfigIndex, string> _configRepository;

    public RankProvider(IGraphQLHelper graphQlHelper, IOptionsSnapshot<ChainOptions> chainOptions,
        ICacheProvider cacheProvider, IContractProvider contractProvider,
        INESTRepository<RaceInfoConfigIndex, string> configRepository)
    {
        _graphQlHelper = graphQlHelper;
        _cacheProvider = cacheProvider;
        _contractProvider = contractProvider;
        _configRepository = configRepository;
        _chainOptions = chainOptions.Value;
    }

    public async Task<WeekRankResultDto> GetWeekRankAsync(int weekNum, string caAddress, int skipCount,
        int maxResultCount)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<WeekRankResultGraphDto>(new GraphQLRequest
        {
            Query = @"
			    query($skipCount:Int!,$maxResultCount:Int!,$caAddress:String!,$weekNum:Int!) {
                    getWeekRank(getRankDto:{skipCount: $skipCount,maxResultCount:$maxResultCount,caAddress:$caAddress,weekNum:$weekNum})
                        {
                              status
                              refreshTime
                              rankingList{
                                rank
                                score
                                caAddress
                                symbol
                                decimals
                              }
                              selfRank{
                                rank
                                score
                                caAddress
                                symbol
                                decimals
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
        return graphQLResponse.GetWeekRank;
    }

    public async Task<GameBlockHeightDto> GetLatestGameByBlockHeightAsync(long blockHeight)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<GameBlockHeightGraphDto>(new GraphQLRequest
        {
            Query = @"
			    query($blockHeight:Long!) {
                    getLatestGameByBlockHeight(getLatestGameHisDto:{
                      blockHeight: $blockHeight
                    }){
                      seasonId
                      latestGameId
                      bingoBlockHeight
                      gameCount
                      bingoTime
                     }
            }",
            Variables = new
            {
                blockHeight
            }
        });
        return graphQLResponse.GetLatestGameByBlockHeight;
    }

    public async Task<List<GameRecordDto>> GetGoRecordsAsync()
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<GameRecordResultDto>(new GraphQLRequest
        {
            Query = @"
			    query {
                  getGoRecords(getGoRecordDto:{goCount:0,skipCount:0,maxResultCount:0}){
                    id
                    caAddress
                    triggerTime
               }
            }"
        });
        return graphQLResponse.GetGoRecords;
    }

    public async Task<int> GetGoCountAsync(GetGoDto dto)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<GameGoCountDto>(new GraphQLRequest
        {
            Query = @"
			    query($startTime:DateTime!,$endTime:DateTime!,$goCount:Int!, $caAddressList:[String!],$skipCount:Int!,$maxResultCount:Int!) {
                   getGoCount(getGoDto:{startTime:$startTime,endTime:$endTime,goCount:$goCount,caAddressList:$caAddressList,skipCount:$skipCount,maxResultCount:$maxResultCount}){
                     goCount
               }
            }",
            Variables = new
            {
                startTime = dto.StartTime,
                endTime = dto.EndTime,
                goCount = dto.GoCount,
                caAddressList = dto.CaAddressList,
                skipCount = dto.SkipCount,
                maxResultCount = dto.MaxResultCount
            }
        });
        return graphQLResponse.GetGoCount?.GoCount ?? 0;
    }

    public async Task<GameHisResultDto> GetGameHistoryListAsync(GetGameHistoryDto dto)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<GameHistoryResultDto>(new GraphQLRequest
        {
            Query = @"
			    query($beginTime:DateTime!,$endTime:DateTime!,$caAddress:String,$skipCount:Int!,$maxResultCount:Int!) {
                  getGameHistoryList(getGameHistoryDto:{beginTime:$beginTime,endTime:$endTime,caAddress:$caAddress,skipCount:$skipCount,maxResultCount:$maxResultCount}){
                    gameList{
                      id
                      caAddress
                      bingoTransactionInfo{
                        triggerTime
                      }
                    }
                }
            }",
            Variables = new
            {
                beginTime = dto.BeginTime,
                endTime = dto.EndTime,
                caAddress = dto.CaAddress,
                skipCount = dto.SkipCount,
                maxResultCount = dto.MaxResultCount
            }
        });
        return graphQLResponse?.GetGameHistoryList;
    }

    public async Task<List<UserBalanceDto>> GetUserBalanceAsync(GetUserBalanceDto dto)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<UserBalanceResultDto>(new GraphQLRequest
        {
            Query = @"
            query($chainId:String!,$address:String!,$symbols:[String!]!) {
              getUserBalanceList(userBalanceDto:{chainId:$chainId,address:$address,symbols:$symbols}){
                symbol
                amount
                }
            }",
            Variables = new
            {
                chainId = dto.ChainId,
                address = dto.CaAddress,
                symbols = dto.Symbols
            }
        });
        return graphQLResponse?.GetUserBalanceList;
    }

    public async Task<RankDto> GetSelfWeekRankAsync(int weekNum, string caAddress)
    {
        var graphQLResponse = await _graphQlHelper.QueryAsync<SelfWeekRankGraphQlDto>(new GraphQLRequest
        {
            Query = @"
            query($weekNum:Int!,$caAddress:String!) {
              getSelfWeekRank(getRankDto:{weekNum:$weekNum,caAddress:$caAddress}){
                  rank
                  score
                  caAddress
                  symbol
                  decimals
                }
            }",
            Variables = new
            {
                weekNum,
                caAddress
            }
        });
        return graphQLResponse?.GetSelfWeekRank;
    }

    public async Task<List<RaceInfoConfigIndex>> GetRaceInfoAsync()
    {
        var result = await _configRepository.GetListAsync();
        return result.Item2;
    }

    public async Task<CurrentRaceInfoCache> GetCurrentRaceInfoAsync()
    {
        var racePri = "CurrentRaceInfo";
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var cacheKey = $"{racePri}:{date}";
        cacheKey = cacheKey.Replace("-", ":");
        var cache = await _cacheProvider.Get<CurrentRaceInfoCache>(cacheKey);

        if (cache != null)
        {
            return cache;
        }

        var raceInfo = await _contractProvider.GetCurrentRaceInfoAsync(_chainOptions.ChainInfos.Keys.First());
        var raceCache = new CurrentRaceInfoCache
        {
            WeekNum = raceInfo.WeekNum,
            CurrentRaceTimeInfo = new CurrentRaceTimeInfo
            {
                BeginTime = raceInfo.RaceTimeInfo.BeginTime.ToDateTime(),
                EndTime = raceInfo.RaceTimeInfo.EndTime.ToDateTime(),
                SettleBeginTime = raceInfo.RaceTimeInfo.SettleBeginTime.ToDateTime(),
                SettleEndTime = raceInfo.RaceTimeInfo.SettleEndTime.ToDateTime()
            }
        };

        await _cacheProvider.Set<CurrentRaceInfoCache>(cacheKey, raceCache, null);
        return raceCache;
    }
}