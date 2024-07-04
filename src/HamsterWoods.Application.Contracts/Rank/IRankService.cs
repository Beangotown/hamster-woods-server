using System.Collections.Generic;
using System.Threading.Tasks;

namespace HamsterWoods.Rank;

public interface IRankService
{
    public Task<WeekRankResultDto> GetWeekRankAsync(GetRankDto getRankDto);
    
    //public Task<RankingHisResultDto> GetRankingHistoryAsync(GetRankingHisDto getRankingHisDto);

    //public Task SyncRankDataAsync();
    public Task SyncGameDataAsync();
    Task<List<GetHistoryDto>> GetHistoryAsync(GetRankDto input);
}