using HamsterWoods.Commons;

namespace HamsterWoods.Grains.Grain;

public class GrainResultDto<T> : GrainResultDto
{
    public T Data { get; set; }
    
    public GrainResultDto()
    {
    }

    public GrainResultDto(T data)
    {
        Data = data;
    }

    public GrainResultDto<T> Error(string message)
    {
        Success = false;
        Message = message;
        return this;
    }
}

public class GrainResultDto
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
}