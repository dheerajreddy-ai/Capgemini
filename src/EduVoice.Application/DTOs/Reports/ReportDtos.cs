namespace EduVoice.Application.DTOs.Reports;

public class StudentReportCardDto
{
    public Guid StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string ParentPhone { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }

    // Academic
    public decimal? MathMarks { get; set; }
    public decimal? ScienceMarks { get; set; }
    public decimal? EnglishMarks { get; set; }
    public decimal? TeluguMarks { get; set; }
    public decimal? SocialMarks { get; set; }
    public decimal? TotalMarks { get; set; }
    public decimal? MaxMarks { get; set; }
    public decimal? Percentage { get; set; }
    public string? Grade { get; set; }
    public string? Remarks { get; set; }

    // Attendance
    public int? AttendancePresentDays { get; set; }
    public int? AttendanceTotalDays { get; set; }
    public decimal? AttendancePercentage { get; set; }

    // Fees
    public decimal TotalFees { get; set; }
    public decimal PaidFees { get; set; }
    public decimal PendingFees { get; set; }
    public string FeesStatus { get; set; } = string.Empty;

    // School
    public string SchoolName { get; set; } = string.Empty;
    public string? SchoolLogoUrl { get; set; }
    public string? SchoolAddress { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class SubjectStat
{
    public string Subject { get; set; } = string.Empty;
    public decimal? AverageMarks { get; set; }
    public decimal? HighestMarks { get; set; }
    public decimal? LowestMarks { get; set; }
    public int StudentCount { get; set; }
}

public class ClassPerformanceDto
{
    public string Class { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public decimal? AveragePercentage { get; set; }
    public decimal? HighestPercentage { get; set; }
    public decimal? LowestPercentage { get; set; }
    public int PassCount { get; set; }
    public decimal PassPercent { get; set; }
    public List<SubjectStat> SubjectStats { get; set; } = [];
}

public class AcademicReportDto
{
    public List<ClassPerformanceDto> ByClass { get; set; } = [];
    public List<TopPerformerDto> TopPerformers { get; set; } = [];
}

public class TopPerformerDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public decimal? Percentage { get; set; }
    public string? Grade { get; set; }
}
