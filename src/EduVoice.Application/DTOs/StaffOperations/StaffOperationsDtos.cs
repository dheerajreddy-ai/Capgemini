namespace EduVoice.Application.DTOs.StaffOperations;

public class StaffAbsenceDto
{
    public Guid Id { get; set; }
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
    public DateTime CreatedAt { get; set; }
}

public class PtmScheduleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime PtmDate { get; set; }
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string? Notes { get; set; }
    public bool Reminder3DaySent { get; set; }
    public bool Reminder1DaySent { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record LogAbsenceRequest(
    string TeacherName,
    string? TeacherPhone,
    string? SubstituteTeacherName,
    string? SubstituteTeacherPhone,
    string? AffectedClass,
    string? AffectedSection,
    DateTime AbsenceDate,
    string? Notes,
    bool NotifyParents
);

public record CreatePtmRequest(
    string Title,
    DateTime PtmDate,
    string? Class,
    string? Section,
    string? Notes
);
