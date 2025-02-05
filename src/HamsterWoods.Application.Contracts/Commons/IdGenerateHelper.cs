using System.Collections.Generic;

namespace HamsterWoods.Commons;

public static class IdGenerateHelper
{
    public static string GenerateId(params object[] inputs)
    {
        return inputs.JoinAsString("-");
    }
}