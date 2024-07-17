using System.Collections.Generic;

namespace HamsterWoods.Rank;

public class UserBalanceResultDto
{
    public List<UserBalanceDto> GetUserBalanceList { get; set; }
}

public class UserBalanceDto
{
    public string Symbol { get; set; }
    
    public long Amount { get; set; }
}