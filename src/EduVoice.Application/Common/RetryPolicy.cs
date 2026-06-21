namespace EduVoice.Application.Common;

public static class RetryPolicy
{
    public const int MaxRetries = 2;
    public static readonly TimeSpan RetryGap = TimeSpan.FromHours(2);

    public static DateTime NextRetryUtc()
    {
        var candidate = DateTime.UtcNow.Add(RetryGap);
        // If next retry lands outside calling window, push to next window open
        return CallingWindowHelper.IsWithinCallingWindow(candidate)
            ? candidate
            : CallingWindowHelper.NextWindowOpen();
    }
}
