using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Campaigns;

public class CreateCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public CampaignType Type { get; set; }
    public string? Description { get; set; }
    public string? FilterClass { get; set; }
    public string? FilterSection { get; set; }
    public FeesStatus? FilterFeesStatus { get; set; }
    public string? CustomMessage { get; set; }
    public DateTime? ScheduledAt { get; set; }
}
