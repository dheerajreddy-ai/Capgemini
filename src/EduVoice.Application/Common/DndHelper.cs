using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Common;

public static class DndHelper
{
    /// <summary>
    /// Checks numbers against TRAI DND registry.
    /// Returns set of numbers that ARE on DND (should be skipped).
    /// In dev mode (no API key) returns empty set — allow all.
    /// Production: replace HTTP call with licensed provider (ValueFirst, Kaleyra, etc).
    /// </summary>
    public static async Task<HashSet<string>> GetDndNumbersAsync(
        IEnumerable<string> phoneNumbers, string? apiKey, ILogger? logger = null)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            // Dev/simulation mode — no DND blocking
            logger?.LogInformation("DND scrub: no API key configured, skipping (dev mode)");
            return new HashSet<string>();
        }

        try
        {
            // TODO: replace with actual licensed DND API call
            // e.g. https://api.valueFirst.com/dnd/check?numbers=...
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            // Placeholder — returns empty (no blocked numbers)
            return new HashSet<string>();
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "DND scrub failed, allowing all numbers (fail-open)");
            return new HashSet<string>();
        }
    }
}
