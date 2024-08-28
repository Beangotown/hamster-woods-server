using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using AElf.Indexing.Elasticsearch.Options;
using HamsterWoods.Cache;
using HamsterWoods.Commons;
using HamsterWoods.Enums;
using HamsterWoods.Grains.Grain.Points;
using HamsterWoods.Info.Dtos;
using HamsterWoods.Points;
using HamsterWoods.Points.Etos;
using HamsterWoods.Rank;
using HamsterWoods.Rank.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Orleans;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Info;

public class InfoAppService : IInfoAppService, ISingletonDependency
{
    private readonly IRankProvider _rankProvider;
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger<InfoAppService> _logger;
    private readonly IndexSettingOptions _indexSettingOptions;
    private readonly IEnumerable<ISearchService> _esServices;
    private readonly IClusterClient _clusterClient;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IObjectMapper _objectMapper;
    private readonly INESTRepository<ContractInvokeIndex, string> _contractInvokeIndexRepository;

    public InfoAppService(IRankProvider rankProvider, ICacheProvider cacheProvider, ILogger<InfoAppService> logger,
        IOptions<IndexSettingOptions> indexSettingOptions, IEnumerable<ISearchService> esServices,
        IClusterClient clusterClient, IDistributedEventBus distributedEventBus, IObjectMapper objectMapper,
        INESTRepository<ContractInvokeIndex, string> contractInvokeIndexRepository)
    {
        _rankProvider = rankProvider;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _indexSettingOptions = indexSettingOptions.Value;
        _esServices = esServices;
        _clusterClient = clusterClient;
        _distributedEventBus = distributedEventBus;
        _objectMapper = objectMapper;
        _contractInvokeIndexRepository = contractInvokeIndexRepository;
    }

    public async Task<CurrentRaceInfoCache> GetCurrentRaceInfoAsync()
    {
        var weekInfo = await _rankProvider.GetCurrentRaceInfoAsync();
        return weekInfo;
    }

    public async Task<object> GetValAsync(string key)
    {
        var val = await _cacheProvider.GetAsync(key);
        if (!val.IsNull) return val.ToString();
        return val;
    }

    public async Task<object> SetValAsync(string key, string val)
    {
        await _cacheProvider.SetAsync(key, val, null);
        return await GetValAsync(key);
    }

    public async Task<string> GetDataAsync(GetIndexDataDto input, string indexName)
    {
        try
        {
            var indexPrefix = _indexSettingOptions.IndexPrefix.ToLower();
            var index = $"{indexPrefix}.{indexName}";

            var esService = _esServices.FirstOrDefault(e => e.IndexName == index);
            if (input.MaxResultCount > 1000)
            {
                input.MaxResultCount = 1000;
            }

            return esService == null ? null : await esService.GetListByLucenceAsync(index, input);
        }
        catch (Exception e)
        {
            _logger.LogError("Search from es error.", e);
            throw;
        }
    }

    public async Task<string> SetBatchUnlockAsync()
    {
        _logger.LogInformation("[SetBatchUnlock] start.");
        var records = await GetRecordsAsync();
        _logger.LogInformation("[SetBatchUnlock] records count:{count}.", records.Count);

        foreach (var record in records)
        {
            await SetBatchUnlockAsync(record.BizId);
        }

        _logger.LogInformation("[SetBatchUnlock] end.");
        return "success";
    }

    private async Task<List<ContractInvokeIndex>> GetRecordsAsync()
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<ContractInvokeIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.Status).Value(ContractInvokeStatus.FinalFailed.ToString())));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ContractMethod).Value(CommonConstant.BatchUnlockAcorns)));
        QueryContainer Filter(QueryContainerDescriptor<ContractInvokeIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await _contractInvokeIndexRepository.GetListAsync(Filter);
        return result.Item2;
    }

    private async Task SetBatchUnlockAsync(string bizId)
    {
        var contractInvokeGrain = _clusterClient.GetGrain<IContractInvokeGrain>(bizId);
        var result = await contractInvokeGrain.ResetUnlock();
        if (!result.Success)
        {
            _logger.LogError(
                "[SetBatchUnlock] Create Contract Invoke fail, bizId: {bizId}.", bizId);
            throw new UserFriendlyException($"SetBatchUnlock fail, bizId: {bizId}.");
        }

        _logger.LogInformation(
            "[SetBatchUnlock]  success, bizId: {bizId}.", bizId);
        await _distributedEventBus.PublishAsync(
            _objectMapper.Map<ContractInvokeGrainDto, ContractInvokeEto>(result.Data));
    }
}