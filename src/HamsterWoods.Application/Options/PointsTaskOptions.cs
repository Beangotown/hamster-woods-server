using System.Collections.Generic;

namespace HamsterWoods.Options;

public class PointsTaskOptions
{
    public Hop Hop { get; set; }
    public Chance Chance { get; set; }
}

public class Hop
{
    public string PointName { get; set; }
    public string ImageUrl { get; set; }
    public List<HopConfig> HopConfigs { get; set; } = new();
}
public class HopConfig
{
    public int HopCount { get; set; }
    public int PointAmount { get; set; }
    public bool IsOverHop { get; set; }
}

public class Chance
{
    public string PointName { get; set; }
    public string ImageUrl { get; set; }
    public List<ChanceConfig> ChanceConfigs { get; set; } = new();
}

public class ChanceConfig
{
    public int FromCount { get; set; }
    public int ToCount { get; set; }
    public int PointAmount { get; set; }
}