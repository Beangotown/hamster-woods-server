using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HamsterWoods.Info;
using HamsterWoods.Options;
using HamsterWoods.Points.Dtos;
using HamsterWoods.Points.Provider;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace HamsterWoods.Points;

public class PointsAppService : IPointsAppService, ISingletonDependency
{
    private readonly IPointsProvider _pointsProvider;
    private readonly IOptionsMonitor<PointsTaskOptions> _options;
    private readonly IObjectMapper _mapper;
    private readonly IInfoAppService _infoAppService;

    public PointsAppService(IPointsProvider pointsProvider, IOptionsMonitor<PointsTaskOptions> options,
        IObjectMapper mapper, IInfoAppService infoAppService)
    {
        _pointsProvider = pointsProvider;
        _options = options;
        _mapper = mapper;
        _infoAppService = infoAppService;
    }

    public async Task<List<DailyDto>> GetDailyListAsync(string caAddress)
    {
        var startTime = DateTime.UtcNow.Date;
        var endTime = DateTime.UtcNow.AddDays(1).Date;
        var hopCount = await _pointsProvider.GetHopCountAsync(startTime, endTime, caAddress);
        var dailyDtoList = _mapper.Map<List<HopConfig>, List<DailyDto>>(_options.CurrentValue.Hop.HopConfigs);
        foreach (var dailyDto in dailyDtoList)
        {
            dailyDto.PointName = _options.CurrentValue.Hop.PointName;
            dailyDto.ImageUrl = _options.CurrentValue.Hop.ImageUrl;
            dailyDto.CurrentHopCount = (int)hopCount.HopCount;
            dailyDto.IsComplete = dailyDto.HopCount <= hopCount.HopCount;
        }

        return dailyDtoList.OrderByDescending(t => !t.IsComplete).ThenBy(m => m.HopCount)
            .ToList();
    }

    public async Task<List<WeeklyDto>> GetWeeklyListAsync(string caAddress)
    {
        var currentRaceInfo = await _infoAppService.GetCurrentRaceInfoAsync();
        var chanceCount = await _pointsProvider.GetPurchaseCountAsync(currentRaceInfo.CurrentRaceTimeInfo.BeginTime,
            currentRaceInfo.CurrentRaceTimeInfo.EndTime, caAddress);

        var weeklyDtoList =
            _mapper.Map<List<ChanceConfig>, List<WeeklyDto>>(_options.CurrentValue.Chance.ChanceConfigs);

        foreach (var weeklyDto in weeklyDtoList)
        {
            weeklyDto.PointName = _options.CurrentValue.Chance.PointName;
            weeklyDto.ImageUrl = _options.CurrentValue.Chance.ImageUrl;
            weeklyDto.CurrentPurchaseCount = (int)chanceCount.PurchaseCount;
            weeklyDto.IsComplete = weeklyDto.ToCount <= chanceCount.PurchaseCount;
        }

        return weeklyDtoList.OrderByDescending(t => !t.IsComplete).ThenBy(t => t.ToCount).ToList();
    }
}