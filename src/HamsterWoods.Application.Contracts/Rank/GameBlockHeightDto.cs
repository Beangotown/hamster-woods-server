using System;
using JetBrains.Annotations;

namespace HamsterWoods.Rank;

public class GameBlockHeightGraphDto
{
    public GameBlockHeightDto GetLatestGameByBlockHeight { get; set; }
}

public class GameBlockHeightDto
{
    public long BingoBlockHeight { get; set; }
    public DateTime? BingoTime { get; set; }
    [CanBeNull] public string LatestGameId { get; set; }
    [CanBeNull] public string SeasonId { get; set; }

    public long GameCount { get; set; }
}