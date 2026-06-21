using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Admin;

public class AdminSchoolDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SubDomain { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public PlanType PlanType { get; set; }
    public int StudentCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalCalls { get; set; }
    public int TotalUsers { get; set; }
}
