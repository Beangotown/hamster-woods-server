using System.Collections.Generic;

namespace HamsterWoods.Points.Dtos;

public class FluxPointResultDto
{
    public List<FluxPointsDto> FluxPointsList { get; set; } = new();
    public bool IsChange { get; set; }
}
public class FluxPointsDto
{
    public string Behavior { get; set; }
    public string PointName { get; set; }
    public int PointAmount { get; set; }
}