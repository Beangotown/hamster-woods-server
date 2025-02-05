using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using AElf.Indexing.Elasticsearch.Options;
using HamsterWoods.Info.Dtos;
using HamsterWoods.Rank;
using HamsterWoods.TokenLock;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using SortType = HamsterWoods.Info.Dtos.SortType;

namespace HamsterWoods.Info;

public interface ISearchService
{
    string IndexName { get; }

    Task<string> GetListByLucenceAsync(string indexName, GetIndexDataDto input);
}

public abstract class SearchService<TEntity, TKey> : ISearchService
    where TEntity : class, IEntity<TKey>, new()
{
    public abstract string IndexName { get; }

    protected readonly INESTRepository<TEntity, TKey> _nestRepository;

    protected SearchService(INESTRepository<TEntity, TKey> nestRepository)
    {
        _nestRepository = nestRepository;
    }

    public virtual async Task<string> GetListByLucenceAsync(string indexName, GetIndexDataDto input)
    {
        Func<SortDescriptor<TEntity>, IPromise<IList<ISort>>> sort = null;
        if (!string.IsNullOrEmpty(input.Sort))
        {
            var sortList = ConvertSortOrder(input.Sort);
            var sortDescriptor = new SortDescriptor<TEntity>();
            sortDescriptor = sortList.Aggregate(sortDescriptor,
                (current, sortType) => current.Field(new Field(sortType.SortField), sortType.SortOrder));
            sort = s => sortDescriptor;
        }

        var (totalCount, items) = await _nestRepository.GetListByLucenceAsync(input.Filter, sort,
            input.MaxResultCount,
            input.SkipCount, indexName);

        var serializeSetting = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        return JsonConvert.SerializeObject(new PagedResultDto<TEntity>
        {
            Items = items,
            TotalCount = totalCount
        }, Formatting.None, serializeSetting);
    }

    protected static IEnumerable<SortType> ConvertSortOrder(string sort)
    {
        var sortList = new List<SortType>();
        foreach (var sortOrder in sort.Split(","))
        {
            var array = sortOrder.Split(" ");
            var order = SortOrder.Ascending;
            if (string.Equals(array.Last(), "asc", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(array.Last(), "ascending", StringComparison.OrdinalIgnoreCase))
            {
                order = SortOrder.Ascending;
            }
            else if (string.Equals(array.Last(), "desc", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(array.Last(), "descending", StringComparison.OrdinalIgnoreCase))
            {
                order = SortOrder.Descending;
            }

            sortList.Add(new SortType
            {
                SortField = array.First(),
                SortOrder = order
            });
        }

        return sortList;
    }
}

public class UserWeekRankRecordSearchService : SearchService<UserWeekRankRecordIndex, string>
{
    private readonly IndexSettingOptions _indexSettingOptions;

    public override string IndexName =>
        $"{_indexSettingOptions.IndexPrefix.ToLower()}.{nameof(UserWeekRankRecordIndex).ToLower()}";

    public UserWeekRankRecordSearchService(INESTRepository<UserWeekRankRecordIndex, string> nestRepository,
        IOptionsSnapshot<IndexSettingOptions> indexSettingOptions) : base(nestRepository)
    {
        _indexSettingOptions = indexSettingOptions.Value;
    }
}

public class RaceInfoConfigSearchService : SearchService<RaceInfoConfigIndex, string>
{
    private readonly IndexSettingOptions _indexSettingOptions;

    public override string IndexName =>
        $"{_indexSettingOptions.IndexPrefix.ToLower()}.{nameof(RaceInfoConfigIndex).ToLower()}";

    public RaceInfoConfigSearchService(INESTRepository<RaceInfoConfigIndex, string> nestRepository,
        IOptionsSnapshot<IndexSettingOptions> indexSettingOptions) : base(nestRepository)
    {
        _indexSettingOptions = indexSettingOptions.Value;
    }
}