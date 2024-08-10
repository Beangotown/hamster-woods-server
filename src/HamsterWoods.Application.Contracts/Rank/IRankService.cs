using System.Collections.Generic;
using System.Threading.Tasks;

namespace HamsterWoods.Rank;

public interface IRankService
{
    Task<WeekRankResultDto> GetWeekRankAsync(GetRankDto getRankDto);
    Task<List<GetHistoryDto>> GetHistoryAsync(GetRankDto input);
}