using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Campaigns;

public class CampaignDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CampaignType Type { get; set; }
    public CampaignStatus Status { get; set; }
    public string? Description { get; set; }
    public string? FilterClass { get; set; }
    public string? FilterSection { get; set; }
    public FeesStatus? FilterFeesStatus { get; set; }
    public int TotalStudents { get; set; }
    public int CallsInitiated { get; set; }
    public int CallsCompleted { get; set; }
    public int CallsFailed { get; set; }
    public int CallsNoAnswer { get; set; }
    public double ProgressPercent { get; set; }
    public string? CustomMessage { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
