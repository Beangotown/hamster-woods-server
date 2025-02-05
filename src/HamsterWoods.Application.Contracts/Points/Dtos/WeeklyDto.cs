namespace HamsterWoods.Points.Dtos;

public class WeeklyDto
{
    public int FromCount { get; set; }
    public int ToCount { get; set; }
    public int CurrentPurchaseCount { get; set; }
    public string PointName { get; set; }
    public int PointAmount { get; set; }
    public bool IsComplete { get; set; }
    public string ImageUrl { get; set; }
}