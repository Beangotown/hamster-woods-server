namespace HamsterWoods.Points.Dtos;

public class DailyDto
{
    public int HopCount { get; set; }
    public int CurrentHopCount { get; set; }
    public string PointName { get; set; }
    public bool IsComplete { get; set; }
    public int PointAmount { get; set; }
    public bool IsOverHop { get; set; }
    public string ImageUrl { get; set; }
}