using System;
using System.Collections.Generic;

namespace HamsterWoods.Trace;

public class GameRecordDto
{
    public string Id { get; set; }
    public string CaAddress { get; set; }
    public DateTime TriggerTime { get; set; }
}

public class GameRecordResultDto
{
    public List<GameRecordDto> GetGoRecords { get; set; }
}