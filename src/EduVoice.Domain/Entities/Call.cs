using EduVoice.Domain.Enums;

namespace EduVoice.Domain.Entities;

public class Call
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public Guid? CampaignId { get; set; }
    public string? VapiCallId { get; set; }
    public CallType Type { get; set; }
    public CallDirection Direction { get; set; }
    public CallStatus Status { get; set; }
    public string ToPhone { get; set; } = string.Empty;
    public string? FromPhone { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string? RecordingUrl { get; set; }
    public string? TranscriptJson { get; set; }
    public string? TranscriptText { get; set; }
    public SentimentType? Sentiment { get; set; }
    public bool FeesConfirmed { get; set; }
    public bool HasComplaint { get; set; }
    public string? ComplaintSummary { get; set; }
    public bool CallbackRequested { get; set; }
    public string? AiSummary { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public CallCampaign? Campaign { get; set; }
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
