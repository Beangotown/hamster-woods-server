using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HamsterWoods.Commons;

public static class HttpHelper
{
    public static string ObjectToGetParam(object obj)
    {
        var strBui = new StringBuilder("?");

        var proArray = obj.GetType().GetProperties();
        foreach (var property in proArray)
        {
            if (property.CanRead)
            {
                strBui.Append($"&{property.Name}={property.GetValue(obj, null)}");
            }
        }

        return strBui.ToString();
    }

    public static string StringArrayToGetParam(string name, List<string> paramList, bool isFirst = false)
    {
        if (paramList == null || paramList.Count == 0)
        {
            return string.Empty;
        }

        paramList = paramList.Where(param => !param.IsNullOrWhiteSpace()).ToList();
        if (paramList.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var param in paramList)
        {
            builder.Append($"&{name}={param}");
        }

        return isFirst ? builder.ToString().ReplaceFirst("&", "?") : builder.ToString();
    }
}