using EduVoice.Domain.Enums;

namespace EduVoice.Domain.Entities;

public class School
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
    public int StudentCount { get; set; }
    public string? TwilioPhoneNumber { get; set; }
    public string? VapiAssistantId { get; set; }
    public string? ElevenLabsVoiceId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? TrialEndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<CallCampaign> Campaigns { get; set; } = new List<CallCampaign>();
    public ICollection<Call> Calls { get; set; } = new List<Call>();
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
