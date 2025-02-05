using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.Points.Dtos;

namespace HamsterWoods.Points;

public interface IPointsAppService
{
    Task<List<DailyDto>> GetDailyListAsync(string caAddress);
    Task<List<WeeklyDto>> GetWeeklyListAsync(string caAddress);
}