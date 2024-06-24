namespace HamsterWoods.Commons;

public class ResponseDto<T>
{
    public string Code { get; set; }
    
    public string Desc { get; set; }
    
    public T Data { get; set; }
    
    public string Message { get; set; }
}