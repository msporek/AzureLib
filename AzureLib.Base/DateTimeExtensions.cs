using System;

namespace AzureLib.Base;

public static class DateTimeExtensions
{
    public static string UtcDateTimeToString(this DateTime utcDateTime)
    {
        string result = utcDateTime.ToString("yyyy/MM/dd HH:mm:ss.fff") + " Z";
        return result;
    }

    public static Int32 ToUnixTimestamp(this DateTime utcDateTime)
    {
        return (Int32)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }

    public static DateTime UnixTimestampToUtcDateTime(Int32 unixTimeStamp)
    {
        return DateTime.UnixEpoch.AddSeconds(unixTimeStamp).ToUniversalTime();        
    }
}
