using EduVoice.Domain.Enums;

namespace EduVoice.Domain.Entities;

public class Broadcast
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public BroadcastMediaType MediaType { get; set; } = BroadcastMediaType.None;
    public string? TargetClass { get; set; }
    public string? TargetSection { get; set; }
    public BroadcastStatus Status { get; set; } = BroadcastStatus.Draft;
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
}
