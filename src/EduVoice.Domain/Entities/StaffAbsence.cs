namespace EduVoice.Domain.Entities;

public class StaffAbsence
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string? TeacherPhone { get; set; }
    public string? SubstituteTeacherName { get; set; }
    public string? SubstituteTeacherPhone { get; set; }
    public string? AffectedClass { get; set; }
    public string? AffectedSection { get; set; }
    public DateTime AbsenceDate { get; set; }
    public string? Notes { get; set; }
    public bool SubstituteAlertSent { get; set; }
    public bool ParentNotificationSent { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
}
