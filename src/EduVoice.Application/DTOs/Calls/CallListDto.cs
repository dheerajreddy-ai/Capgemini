using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Calls;

public class CallListDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string ToPhone { get; set; } = string.Empty;
    public CallType Type { get; set; }
    public CallStatus Status { get; set; }
    public CallDirection Direction { get; set; }
    public int? DurationSeconds { get; set; }
    public SentimentType? Sentiment { get; set; }
    public bool HasComplaint { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
