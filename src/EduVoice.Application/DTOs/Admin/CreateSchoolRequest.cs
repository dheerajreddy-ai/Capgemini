using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Admin;

public class CreateSchoolRequest
{
    public string SchoolName { get; set; } = string.Empty;
    public string SubDomain { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public PlanType PlanType { get; set; }
    public string AdminFirstName { get; set; } = string.Empty;
    public string AdminLastName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public DateTime? TrialEndsAt { get; set; }
}
