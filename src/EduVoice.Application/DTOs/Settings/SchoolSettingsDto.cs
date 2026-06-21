using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Settings;

public class SchoolSettingsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string SubDomain { get; set; } = string.Empty;
    public string? PrimaryColor { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public PlanType PlanType { get; set; }
    public string? TwilioPhoneNumber { get; set; }
    public string? VapiAssistantId { get; set; }
    public string? ElevenLabsVoiceId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? TrialEndsAt { get; set; }
}
