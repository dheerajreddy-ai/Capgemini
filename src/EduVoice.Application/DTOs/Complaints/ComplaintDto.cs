using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Complaints;

public class ComplaintDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public Guid? CallId { get; set; }
    public ComplaintCategory Category { get; set; }
    public ComplaintPriority Priority { get; set; }
    public ComplaintStatus Status { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? DetailedDescription { get; set; }
    public string? ParentName { get; set; }
    public string? ParentPhone { get; set; }
    public string? Resolution { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? SlaDeadline { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public int EscalationLevel { get; set; }
    public bool IsOverdue => SlaDeadline.HasValue && DateTime.UtcNow > SlaDeadline.Value
        && Status != ComplaintStatus.Resolved && Status != ComplaintStatus.Closed;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
