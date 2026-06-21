using EduVoice.Domain.Enums;

namespace EduVoice.Domain.Entities;

public class Student
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string ParentPhone { get; set; } = string.Empty;
    public string? ParentPhone2 { get; set; }
    public string? ParentEmail { get; set; }
    public string? Address { get; set; }

    // Academic marks
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
    public FeesStatus FeesStatus { get; set; }
    public DateTime? FeesDueDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }

    // Phase 1 — compliance
    public bool DoNotCall { get; set; }
    public DateTime? DoNotCallSetAt { get; set; }
    public bool HasFeeExtension { get; set; }
    public DateTime? FeeExtensionUntil { get; set; }

    // Phase 4 — payment
    public string? PaymentLink { get; set; }
    public DateTime? PaymentLinkGeneratedAt { get; set; }
    public bool HasFeeDispute { get; set; }
    public string? FeeDisputeNote { get; set; }
    public DateTime? FeeDisputeRaisedAt { get; set; }

    // Phase 14 — fee intelligence
    public bool IsScholarship { get; set; }
    public string? ScholarshipNote { get; set; }
    public decimal ScholarshipPercent { get; set; }
    public DefaulterEscalationLevel DefaulterEscalationLevel { get; set; }
    public DateTime? DefaulterEscalatedAt { get; set; }
    public bool NeedsPersonalFollowup { get; set; }

    // Phase 10/11 — alerts
    public DateTime? LowMarksAlertSentAt { get; set; }

    // Phase 13 — dropout risk
    public int DropoutRiskScore { get; set; }
    public DropoutRiskLevel DropoutRiskLevel { get; set; }
    public DateTime? DropoutRiskCalculatedAt { get; set; }
    public string? DropoutRiskReasons { get; set; }

    // Phase 5 — parent portal
    public string? PortalOtpHash { get; set; }
    public DateTime? PortalOtpExpiresAt { get; set; }
    public DateTime? LastPortalAccessAt { get; set; }
    public bool MissedCallCallbackPending { get; set; }
    public DateTime? MissedCallReceivedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
    public ICollection<Call> Calls { get; set; } = new List<Call>();
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
