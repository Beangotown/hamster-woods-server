using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch.Options;
using HamsterWoods.Cache;
using HamsterWoods.Info.Dtos;
using HamsterWoods.Rank;
using HamsterWoods.Rank.Provider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Info;

public class InfoAppService : IInfoAppService, ISingletonDependency
{
    private readonly IRankProvider _rankProvider;
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger<InfoAppService> _logger;
    private readonly IndexSettingOptions _indexSettingOptions;
    private readonly IEnumerable<ISearchService> _esServices;

    public InfoAppService(IRankProvider rankProvider, ICacheProvider cacheProvider, ILogger<InfoAppService> logger,
        IOptions<IndexSettingOptions> indexSettingOptions, IEnumerable<ISearchService> esServices)
    {
        _rankProvider = rankProvider;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _indexSettingOptions = indexSettingOptions.Value;
        _esServices = esServices;
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
}