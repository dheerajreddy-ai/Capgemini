using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.Students;

public class StudentListDto
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
    public decimal PendingFees { get; set; }
    public FeesStatus FeesStatus { get; set; }
    public decimal? AttendancePercentage { get; set; }
    public DateTime? FeesDueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
