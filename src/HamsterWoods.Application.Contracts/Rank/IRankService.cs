using System.Collections.Generic;
using System.Threading.Tasks;

namespace HamsterWoods.Rank;

public interface IRankService
{
    public Task<WeekRankResultDto> GetWeekRankAsync(GetRankDto getRankDto);

    public Task<SeasonRankResultDto> GetSeasonRankAsync(GetRankDto getRankDt);

    public Task<SeasonResultDto> GetSeasonConfigAsync();
    public Task<RankingHisResultDto> GetRankingHistoryAsync(GetRankingHisDto getRankingHisDto);

    public Task SyncRankDataAsync();
    public Task SyncGameDataAsync();
    Task<List<GetHistoryDto>> GetHistoryAsync(GetRankDto input);
}