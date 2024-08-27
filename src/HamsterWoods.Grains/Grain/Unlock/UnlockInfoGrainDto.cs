namespace HamsterWoods.Grains.Grain.Unlock;

public class UnlockInfoGrainDto
{
    public string Id { get; set; }
    public int WeekNum { get; set; }
    public string BizId { get; set; }
    public List<string> Addresses { get; set; } = new();
    public DateTime CreateTime { get; set; }
}