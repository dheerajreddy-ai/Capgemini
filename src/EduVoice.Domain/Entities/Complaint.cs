using EduVoice.Domain.Enums;

namespace EduVoice.Domain.Entities;

public class Complaint
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Call? Call { get; set; }
}
