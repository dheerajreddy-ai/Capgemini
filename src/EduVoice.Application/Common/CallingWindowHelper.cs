namespace EduVoice.Application.Common;

public static class CallingWindowHelper
{
    // 11 AM – 6 PM IST (UTC+5:30)
    private static readonly TimeZoneInfo Ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
    private static readonly TimeOnly WindowOpen  = new(11, 0);
    private static readonly TimeOnly WindowClose = new(18, 0);

    public static bool IsWithinCallingWindow()
    {
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);
        var tod = TimeOnly.FromDateTime(now);
        return tod >= WindowOpen && tod < WindowClose;
    }

    public static bool IsWithinCallingWindow(DateTime utcTime)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcTime, Ist);
        var tod = TimeOnly.FromDateTime(local);
        return tod >= WindowOpen && tod < WindowClose;
    }

    public static DateTime NextWindowOpen()
    {
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);
        var today = now.Date.Add(WindowOpen.ToTimeSpan());
        var candidate = today > now ? today : today.AddDays(1);
        return TimeZoneInfo.ConvertTimeToUtc(candidate, Ist);
    }

    public static string WindowDescription => "11:00 AM – 6:00 PM IST";
}
