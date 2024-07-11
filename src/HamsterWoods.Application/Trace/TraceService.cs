using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using HamsterWoods.Contract;
using HamsterWoods.Rank;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Trace;

[RemoteService(false), DisableAuditing]
public class TraceService : HamsterWoodsBaseService, ITraceService
{
    private readonly INESTRepository<UserActionIndex, string> _userActionRepository;
    private readonly IRankProvider _rankProvider;
    private readonly ILogger<TraceService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly ChainOptions _chainOptions;

    private const int CaAddressQueryOnceLimit = 50;
    private const int QueryOnceLimit = 1000;
    private const int QueryMaxLimit = 8000;
    private const string CaAddressPrefix = "ELF";
    private const string DateFormat = "yyyy-MM-dd";
    private const string StartTime = "00:00:00";
    private const string EndTime = "23:59:59";
    private readonly int[] GoArray = { 1, 5, 1 };

    public TraceService(INESTRepository<UserActionIndex, string> userActionRepository,
        IRankProvider rankProvider,
        ILogger<TraceService> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ChainOptions> chainOptions)
    {
        _userActionRepository = userActionRepository;
        _rankProvider = rankProvider;
        _logger = logger;
        _objectMapper = objectMapper;
        _chainOptions = chainOptions.Value;
    }


    public async Task CreateAsync(GetUserActionDto dto)
    {
        _logger.LogInformation("start tracking! caAddress: {caAddress}", dto.CaAddress);
        var userActionIndex = _objectMapper.Map<GetUserActionDto, UserActionIndex>(dto);
        userActionIndex.ChainId = GetDefaultChainId();
        userActionIndex.Timestamp = DateTime.UtcNow;
        userActionIndex.Id =
            $"{userActionIndex.CaAddress}_{userActionIndex.ChainId}_{DateTimeHelper.ToUnixTimeMilliseconds(userActionIndex.Timestamp)}";
        userActionIndex.ActionType =
            (await GetCountAsync(dto.CaAddress)) == 0 ? UserActionType.Register : UserActionType.Login;

        await _userActionRepository.AddOrUpdateAsync(userActionIndex);
    }

    public async Task<StatResultDto> GetStatAsync(GetStatDto getStatDto)
    {
        _logger.LogInformation("start statistics!");
        var chainId = GetDefaultChainId();

        var dto = new GetGoDto();
        if (getStatDto.Type == 0)
        {
            var yesterday = DateTimeHelper.DatetimeToString(DateTime.Today.AddDays(-1), DateFormat);
            dto.StartTime = DateTimeHelper.ParseDateTimeByStr($"{yesterday} {StartTime}").AddHours(-8);
            dto.EndTime = DateTimeHelper.ParseDateTimeByStr($"{yesterday} {EndTime}").AddHours(-8);
        }
        else if (getStatDto.Type == 1)
        {
            dto.StartTime = DateTimeHelper
                .ParseDateTimeByStr(
                    $"{DateTimeHelper.DatetimeToString(DateTime.Today.AddDays(-7), DateFormat)} {StartTime}")
                .AddHours(-8);
            dto.EndTime = DateTimeHelper
                .ParseDateTimeByStr(
                    $"{DateTimeHelper.DatetimeToString(DateTime.Today.AddDays(-1), DateFormat)} {EndTime}")
                .AddHours(-8);
        }

        dto.SkipCount = 0;
        dto.MaxResultCount = QueryMaxLimit;
        var result = new StatResultDto();
        var count = 0;
        for (var i = 0; i < GoArray.Length; i++)
        {
            count = 0;
            dto.GoCount = i == 1 ? GoArray[1] : GoArray[0];
            var caAddressList = await GetCaAddressListAsync(dto.StartTime, dto.EndTime, i == 0 ? 0 : null);

            if (dto.CaAddressList == null) dto.CaAddressList = new List<string>();

            for (var j = 0; j < caAddressList.Count; j++)
            {
                dto.CaAddressList.Add($"{CaAddressPrefix}_{caAddressList[j]}_{chainId}");
                if (dto.CaAddressList.Count == CaAddressQueryOnceLimit)
                {
                    count += await _rankProvider.GetGoCountAsync(dto);
                    dto.CaAddressList.Clear();
                }
            }

            if (dto.CaAddressList.Count > 0 && dto.CaAddressList.Count < CaAddressQueryOnceLimit)
            {
                count += await _rankProvider.GetGoCountAsync(dto);
                dto.CaAddressList.Clear();
            }

            if (i == 0)
            {
                if (getStatDto.Type == 0)
                {
                    result.RegisterCountByDay = caAddressList.Count;
                    result.OneGoCountByDay = count;
                    result.RegisterConvertRateByDay =
                        caAddressList.Count == 0 ? 0 : (double)count / caAddressList.Count;
                }
                else if (getStatDto.Type == 1)
                {
                    result.RegisterCountByWeek = caAddressList.Count;
                    result.OneGoCountByWeek = count;
                    result.RegisterConvertRateByWeek =
                        caAddressList.Count == 0 ? 0 : (double)count / caAddressList.Count;
                }
            }
            else if (i == 1)
            {
                if (getStatDto.Type == 0)
                {
                    result.LoginCountByDay = caAddressList.Count;
                    result.FiveGoCountByDay = count;
                    result.ActivityRateByDay = caAddressList.Count == 0 ? 0 : (double)count / caAddressList.Count;
                }
                else if (getStatDto.Type == 1)
                {
                    result.LoginCountByWeek = caAddressList.Count;
                    result.FiveGoCountByWeek = count;
                    result.ActivityRateByWeek =
                        caAddressList.Count == 0 ? 0 : (double)count / caAddressList.Count;
                }
            }
            else
            {
                if (getStatDto.Type == 0)
                {
                    result.OneGoLoginCountByDay = count;
                }
                else if (getStatDto.Type == 1)
                {
                    result.OneGoLoginCountByWeek = count;
                }
            }
        }

        if (getStatDto.Type == 1)
        {
            count = 0;
            var startTime = DateTimeHelper
                .ParseDateTimeByStr(
                    $"{DateTimeHelper.DatetimeToString(DateTime.Today.AddDays(-14), DateFormat)} {StartTime}")
                .AddHours(-8);
            var endTime = DateTimeHelper
                .ParseDateTimeByStr(
                    $"{DateTimeHelper.DatetimeToString(DateTime.Today.AddDays(-8), DateFormat)} {EndTime}")
                .AddHours(-8);
            dto.GoCount = GoArray[0];

            var caAddressList = await GetCaAddressListAsync(startTime, endTime, 0);

            if (dto.CaAddressList == null) dto.CaAddressList = new List<string>();

            for (var j = 0; j < caAddressList.Count; j++)
            {
                dto.CaAddressList.Add($"{CaAddressPrefix}_{caAddressList[j]}_{chainId}");
                if (dto.CaAddressList.Count == CaAddressQueryOnceLimit)
                {
                    count += await _rankProvider.GetGoCountAsync(dto);
                    dto.CaAddressList.Clear();
                }
            }

            if (dto.CaAddressList.Count > 0 && dto.CaAddressList.Count < CaAddressQueryOnceLimit)
            {
                count += await _rankProvider.GetGoCountAsync(dto);
                dto.CaAddressList.Clear();
            }

            result.LoginAddressCountByWeek = caAddressList.Count;
            result.OneGoAddressCountByWeek = count;
            result.RetentionRateByWeek =
                caAddressList.Count == 0 ? 0 : (double)count / caAddressList.Count;
        }

        return result;
    }

    private string GetDefaultChainId()
    {
        return _chainOptions.ChainInfos.Keys.First();
    }

    private async Task<long> GetCountAsync(string caAddress)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserActionIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CaAddress).Value(caAddress)));

        QueryContainer Filter(QueryContainerDescriptor<UserActionIndex> f)
        {
            return f.Bool(b => b.Must(mustQuery));
        }

        var result = await _userActionRepository.CountAsync(Filter);
        return result.Count;
    }

    private async Task<List<string>> GetCaAddressListAsync(DateTime? startTime, DateTime? endTime, int? type)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserActionIndex>, QueryContainer>>();
        if (startTime != null)
        {
            mustQuery.Add(q => q.DateRange(i => i.Field(f => f.Timestamp).GreaterThanOrEquals(startTime)));
        }

        if (endTime != null)
        {
            mustQuery.Add(q => q.DateRange(i => i.Field(f => f.Timestamp).LessThanOrEquals(endTime)));
        }

        if (type != null)
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.ActionType).Value(type)));
        }

        QueryContainer Filter(QueryContainerDescriptor<UserActionIndex> f)
        {
            return f.Bool(b => b.Must(mustQuery));
        }

        var skipCount = 0;
        var userActionList = new List<UserActionIndex>();
        Tuple<long, List<UserActionIndex>> result = null;
        do
        {
            result = await _userActionRepository.GetListAsync(Filter, skip: skipCount, limit: QueryOnceLimit);
            if (result.Item2.Count == 0) break;
            userActionList.AddRange(result.Item2);
            skipCount += QueryOnceLimit;
        } while (result.Item2.Count >= QueryOnceLimit);

        return userActionList.Select(a => a.CaAddress).Distinct().ToList();
    }
}