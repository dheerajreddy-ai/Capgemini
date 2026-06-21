using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.DropoutRisk;

public class DropoutRiskStudentDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string ParentPhone { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public DropoutRiskLevel RiskLevel { get; set; }
    public List<string> RiskReasons { get; set; } = [];
    public decimal? AttendancePercentage { get; set; }
    public decimal? AcademicPercentage { get; set; }
    public FeesStatus FeesStatus { get; set; }
    public decimal PendingFees { get; set; }
    public int NoAnswerCallsLast30Days { get; set; }
    public DateTime? CalculatedAt { get; set; }
}

public class DropoutRiskSummaryDto
{
    public int TotalStudents { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int LowCount { get; set; }
    public List<DropoutRiskStudentDto> AtRiskStudents { get; set; } = [];
}
