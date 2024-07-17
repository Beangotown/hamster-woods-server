using System.Collections.Generic;
using System.Threading.Tasks;
using HamsterWoods.TokenLock;
using HamsterWoods.Trace;

namespace HamsterWoods.Rank.Provider;

public interface IRankProvider
{
    public Task<WeekRankResultDto> GetWeekRankAsync(int weekNum, string caAddress, int skipCount, int maxResultCount);

    public Task<GameBlockHeightDto> GetLatestGameByBlockHeightAsync(long blockHeight);

    public Task<List<GameRecordDto>> GetGoRecordsAsync();

    public Task<int> GetGoCountAsync(GetGoDto dto);

    public Task<GameHisResultDto> GetGameHistoryListAsync(GetGameHistoryDto dto);

    public Task<List<UserBalanceDto>> GetUserBalanceAsync(GetUserBalanceDto dto);

    Task<CurrentRaceInfoCache> GetCurrentRaceInfoAsync();
    Task<RankDto> GetSelfWeekRankAsync(int weekNum, string caAddress); 
    Task<List<RaceInfoConfigIndex>> GetRaceInfoAsync(); 
}