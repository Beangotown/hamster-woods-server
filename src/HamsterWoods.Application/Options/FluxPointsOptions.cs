using System.Collections.Generic;

namespace HamsterWoods.Options;

public class FluxPointsOptions
{
    public string Graphql { get; set; }
    
    public Dictionary<string, FluxPoint> PointsInfos { get; set; } = new();
    public int Period { get; set; } = 3;
}

public class FluxPoint
{
    public string PointName { get; set; }
    public string Behavior { get; set; }
}