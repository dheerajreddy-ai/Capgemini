using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Settings;

public class UpdateSettingsRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? VapiAssistantId { get; set; }
    public string? ElevenLabsVoiceId { get; set; }
    // Phase 3
    public TeluguDialect TeluguDialect { get; set; }
    public string? ElevenLabsVoiceIdAndhra { get; set; }
    // Phase 4
    public string? UpiId { get; set; }
    // Phase 6
    public string? UrduVoiceId { get; set; }
    public CallLanguage DefaultCallLanguage { get; set; }
    public decimal AttendanceAlertThreshold { get; set; } = 75;
    public bool DndScrubEnabled { get; set; } = true;
    // Module 28
    public string? PrincipalEmail { get; set; }
    public string? PrincipalWhatsApp { get; set; }
    public bool DailySummaryEnabled { get; set; } = false;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
