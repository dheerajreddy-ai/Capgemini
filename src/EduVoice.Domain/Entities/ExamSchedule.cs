using EduVoice.Domain.Enums;

namespace EduVoice.Domain.Entities;

public class ExamSchedule
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public ExamType ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string? Notes { get; set; }
    // Track which reminder waves have been sent
    public bool Reminder3DaySent { get; set; }
    public bool Reminder1DaySent { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
}
