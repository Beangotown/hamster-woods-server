namespace HamsterWoods.Hubs;

public class HubResponse<T>
{
    public HubResponse(){}

    public HubResponse(T body)
    {
        Body = body;
    }

    public T Body { get; set; }
}