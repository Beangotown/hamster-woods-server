using System;
using System.Collections.Generic;

namespace HamsterWoods.Commons;

public static class CommonResult
{
    public const string SuccessCode = "20000";

    public const string UserNotExistCode = "30001";
    public const string UserExistCode = "30002";

    public static readonly Dictionary<string, string> ResponseMappings = new()
    {
        { "20000", "success" },
        { "30001", "user not exist" },
        { "30002", "user already exists" }
    };

    public static string GetMessage(string code)
    {
        if (code.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        return ResponseMappings.GetOrDefault(code);
    }
}