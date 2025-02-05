using System;
using System.Globalization;

namespace HamsterWoods;

public class DateTimeHelper
{
    private static readonly string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    

    public static long ToUnixTimeMilliseconds(DateTime value)
    {
        var span = value - DateTime.UnixEpoch;
        return (long)span.TotalMilliseconds;
    }
    

    public static DateTime FromUnixTimeSeconds(long value)
    {
        return DateTime.UnixEpoch.AddMilliseconds(value * 1000);
    }
    

    public static DateTime ParseDateTimeByStr(string time)
    {
        return DateTime.ParseExact(time, _dateTimeFormat, CultureInfo.InvariantCulture);
    }

    public static string DatetimeToString(DateTime time)
    {
        return time.ToString(_dateTimeFormat);
    }
    
    public static string DatetimeToString(DateTime time, string dateTimeFormat)
    {
        return time.ToString(dateTimeFormat, CultureInfo.InvariantCulture);
    }
}