namespace HamsterWoods.Grains.State.UserPoints;

public class UserPointsState
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    public string Address { get; set; }

    //key : date, value: id
    public Dictionary<string, List<string>> Hop { get; set; } = new();
    
    //key : weekNum, value: id
    public Dictionary<int, List<string>> Purchase { get; set; } = new();
}