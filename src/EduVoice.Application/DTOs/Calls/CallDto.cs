using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Calls;

public class CallDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public Guid? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public CallType Type { get; set; }
    public CallDirection Direction { get; set; }
    public CallStatus Status { get; set; }
    public string ToPhone { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public SentimentType? Sentiment { get; set; }
    public bool FeesConfirmed { get; set; }
    public bool HasComplaint { get; set; }
    public bool CallbackRequested { get; set; }
    public string? AiSummary { get; set; }
    public DateTime CreatedAt { get; set; }
}
