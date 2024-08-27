using System;
using System.Collections.Generic;

namespace HamsterWoods.Unlock.Etos;

public class UnlockInfoEto
{
    public string Id { get; set; }
    public int WeekNum { get; set; }
    public string BizId { get; set; }
    public List<string> Addresses { get; set; } = new();
    public DateTime CreateTime { get; set; }
}