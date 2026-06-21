using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.ExamSchedules;

public class ExamScheduleDto
{
    public Guid Id { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public ExamType ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string? Notes { get; set; }
    public bool Reminder3DaySent { get; set; }
    public bool Reminder1DaySent { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExamScheduleRequest
{
    public string SubjectName { get; set; } = string.Empty;
    public ExamType ExamType { get; set; } = ExamType.UnitTest;
    public DateTime ExamDate { get; set; }
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string? Notes { get; set; }
}
