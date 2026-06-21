using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Complaints;

public class UpdateComplaintRequest
{
    public ComplaintStatus Status { get; set; }
    public ComplaintPriority Priority { get; set; }
    public string? Resolution { get; set; }
    public Guid? AssignedToUserId { get; set; }
}
