using System.Collections.Generic;

namespace HamsterWoods.NFT;

public class HalloweenActivityOptions
{
    public string BeginTime { get; set; } = "2023-10-24 00:00:00";
    public string EndTime { get; set; } = "2023-10-29 00:00:00";

    public List<string> BeanPass { get; set; } = new();
}