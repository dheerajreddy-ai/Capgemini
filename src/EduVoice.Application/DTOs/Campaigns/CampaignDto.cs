using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Campaigns;

public class CampaignDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CampaignType Type { get; set; }
    public CampaignStatus Status { get; set; }
    public string? Description { get; set; }
    public int TotalStudents { get; set; }
    public int CallsInitiated { get; set; }
    public int CallsCompleted { get; set; }
    public int CallsFailed { get; set; }
    public int CallsNoAnswer { get; set; }
    public double ProgressPercent { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
