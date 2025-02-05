using System;
using System.Collections.Generic;

namespace HamsterWoods.EntityEventHandler.Core.Services.Dtos;

public class GetRankRecordsResultDto
{
    public RankRecordsResultDto GetWeekRankRecords { get; set; }
}

public class RankRecordsResultDto
{
    public List<RankRecordDto> RankRecordList { get; set; }
}

public class RankRecordDto
{
    public string CaAddress { get; set; }
    public long SumScore { get; set; }
    public string Symbol { get; set; }
    public int Decimals { get; set; }
    public int WeekNum { get; set; }
    public DateTime UpdateTime { get; set; }
}