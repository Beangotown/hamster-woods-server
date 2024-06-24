namespace HamsterWoods.Entities.Redis;

public class UnreadMessage
{
    public string UserId { get; set; }
    public string AppId { get; set; }
    public string MessageType { get; set; }
    public int UnreadCount { get; set; }

    public string GetKey()
    {
        return UserId + "-" + AppId + "-" + MessageType;
    }
}