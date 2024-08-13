using System.Collections.Generic;

namespace HamsterWoods.Options;

public class PointTradeOptions
{
    public int MaxBatchSize { get; set; } = 20;

    public Dictionary<string, ContractInfo> ContractInfos { get; set; } = new();

    //key is point name
    public Dictionary<string, PointInfo> PointMapping { get; set; } = new();
    
    public ContractInfo GetContractInfo(string chainId)
    {
        return ContractInfos.TryGetValue(chainId, out var chainInfo) ? chainInfo : null;
    }
    
    public string GetActionName(string pointName)
    {
        return PointMapping.TryGetValue(pointName, out var pointInfo) ? pointInfo.ActionName : null;
    }
}

public class ContractInfo
{
    public string HamsterWoodsAddress { get; set; }

    public string MethodName { get; set; }
}

public class PointInfo
{
    public string ActionName { get; set; }

    public string ConditionalExp { get; set; }
    
    public decimal? Factor { get; set; }
    
    public bool NeedMultiplyPrice { get; set; }
    
    public bool UseBalance { get; set; }
}