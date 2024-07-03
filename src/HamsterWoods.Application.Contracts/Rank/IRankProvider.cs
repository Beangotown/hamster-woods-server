using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.Trace;

namespace HamsterWoods.Rank;

public interface IRankProvider
{
    public Task<WeekRankResultDto> GetWeekRankAsync(int weekNum,string caAddress, int skipCount, int maxResultCount);

    public Task<SeasonRecordDto> GetSeasonConfigAsync();

    public Task<SeasonDto> GetSeasonAsync(string seasonId);

    public Task<GameBlockHeightDto> GetLatestGameByBlockHeightAsync(long blockHeight);

    public Task<WeekRankRecordDto>
        GetWeekRankRecordsAsync(string seasonId, int week, int skipCount, int maxResultCount);

    public Task<SeasonRankRecordDto> GetSeasonRankRecordsAsync(string seasonId, int skipCount, int maxResultCount);

    public Task<RankingHisResultDto> GetRankingHistoryAsync(GetRankingHisDto getRankingHisDto);
    
    public Task<List<GameRecordDto>> GetGoRecordsAsync();

    public Task<int> GetGoCountAsync(GetGoDto dto);

    public Task<GameHisResultDto> GetGameHistoryListAsync(GetGameHistoryDto dto);

    public Task<List<UserBalanceDto>> GetUserBalanceAsync(GetUserBalanceDto dto);
}