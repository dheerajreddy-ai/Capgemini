using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Teacher;

public class TeacherStudentDto
{
    public Guid Id { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Class { get; set; }
    public string? Section { get; set; }
    public decimal? AttendancePercentage { get; set; }
    public decimal? MathMarks { get; set; }
    public decimal? ScienceMarks { get; set; }
    public decimal? EnglishMarks { get; set; }
    public decimal? TeluguMarks { get; set; }
    public decimal? SocialMarks { get; set; }
    public decimal? Percentage { get; set; }
    public string? Grade { get; set; }
}

public class ClassSectionDto
{
    public string Class { get; set; } = string.Empty;
    public string? Section { get; set; }
    public int StudentCount { get; set; }
}

public class TeacherDashboardDto
{
    public int TotalStudents { get; set; }
    public int TodayPresent { get; set; }
    public int TodayAbsent { get; set; }
    public bool AttendanceMarkedToday { get; set; }
    public int ActiveHomeworkCount { get; set; }
    public int ClassSectionCount { get; set; }
    public List<ClassSectionDto> ClassSections { get; set; } = new();
}

// --- Attendance ---

public class AttendanceEntryRequest
{
    public Guid StudentId { get; set; }
    public bool IsPresent { get; set; }
    public string? Remarks { get; set; }
}

public class MarkAttendanceRequest
{
    public DateTime Date { get; set; }
    public string Class { get; set; } = string.Empty;
    public string? Section { get; set; }
    public List<AttendanceEntryRequest> Entries { get; set; } = new();
}

public class AttendanceEntryDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool IsPresent { get; set; }
    public string? Remarks { get; set; }
}

public class AttendanceResultDto
{
    public DateTime Date { get; set; }
    public string Class { get; set; } = string.Empty;
    public string? Section { get; set; }
    public int TotalStudents { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public bool AlreadySaved { get; set; }
    public List<AttendanceEntryDto> Entries { get; set; } = new();
}

// --- Marks ---

public class StudentMarksEntryRequest
{
    public Guid StudentId { get; set; }
    public decimal? MathMarks { get; set; }
    public decimal? ScienceMarks { get; set; }
    public decimal? EnglishMarks { get; set; }
    public decimal? TeluguMarks { get; set; }
    public decimal? SocialMarks { get; set; }
}

public class UploadMarksRequest
{
    public ExamType ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    public string Class { get; set; } = string.Empty;
    public string? Section { get; set; }
    public decimal MaxMarksPerSubject { get; set; } = 100;
    public List<StudentMarksEntryRequest> Entries { get; set; } = new();
}

// --- Homework ---

public class AssignHomeworkRequest
{
    public string Class { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}
