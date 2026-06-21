using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Broadcasts;

public class BroadcastDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public BroadcastMediaType MediaType { get; set; }
    public string? TargetClass { get; set; }
    public string? TargetSection { get; set; }
    public BroadcastStatus Status { get; set; }
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBroadcastRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public BroadcastMediaType MediaType { get; set; } = BroadcastMediaType.None;
    public string? TargetClass { get; set; }
    public string? TargetSection { get; set; }
    public bool SendNow { get; set; } = true;
}
