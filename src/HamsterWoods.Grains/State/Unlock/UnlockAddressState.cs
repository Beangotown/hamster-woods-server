namespace HamsterWoods.Grains.State.Unlock;

public class UnlockAddressState
{
    public string Id { get; set; }
    public int WeekNum { get; set; }
    public List<string> Addresses { get; set; } = new();
}