using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Students;

public class StudentDto
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string ParentPhone { get; set; } = string.Empty;
    public string? ParentPhone2 { get; set; }
    public string? ParentEmail { get; set; }
    public string? Address { get; set; }
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
    public int? AttendancePresentDays { get; set; }
    public int? AttendanceTotalDays { get; set; }
    public decimal? AttendancePercentage { get; set; }
    public decimal TotalFees { get; set; }
    public decimal PaidFees { get; set; }
    public decimal PendingFees { get; set; }
    public FeesStatus FeesStatus { get; set; }
    public DateTime? FeesDueDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
