using System;
using System.Collections.Generic;

namespace HamsterWoods.Trace;

public class GameHistoryResultDto
{
    public GameHisResultDto GetGameHistoryList { get; set; }
}

public class GameHisResultDto
{
    public List<GameResultDto> GameList { get; set; }
}

public class GameResultDto
{
    public string Id { get; set; }
    public string CaAddress { get; set; }
    public int GridNum { get; set; }
    public int Score { get; set; }
    public long TranscationFee { get; set; }
    public TransactionInfoDto? PlayTransactionInfo { get; set; }
    public TransactionInfoDto? BingoTransactionInfo { get; set; }
}

public class TransactionInfoDto
{
    public string TransactionId { get; set; }
    public long TransactionFee { get; set; }
    public DateTime TriggerTime { get; set; }
}