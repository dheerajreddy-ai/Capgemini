using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Calls;

public class CallDetailDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public Guid? CampaignId { get; set; }
    public string? CampaignName { get; set; }
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
    public List<TranscriptMessageDto> Transcript { get; set; } = new();
    public string? TranscriptText { get; set; }
    public SentimentType? Sentiment { get; set; }
    public bool FeesConfirmed { get; set; }
    public bool HasComplaint { get; set; }
    public string? ComplaintSummary { get; set; }
    public bool CallbackRequested { get; set; }
    public string? AiSummary { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public bool IsVoicemail { get; set; }
    public bool IsPartialTranscript { get; set; }
    public DateTime? RetryScheduledAt { get; set; }
    public bool EscalationRequired { get; set; }
    public string? EscalationReason { get; set; }
    public bool LowConfidenceTranscript { get; set; }
    public string? DialectUsed { get; set; }
    public string? NetworkQuality { get; set; }
    public DateTime CreatedAt { get; set; }
}
