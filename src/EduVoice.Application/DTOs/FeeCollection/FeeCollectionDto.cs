using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.FeeCollection;

public class FeeCollectionDashboardDto
{
    public decimal TotalFeesExpected { get; set; }
    public decimal TotalFeesCollected { get; set; }
    public decimal TotalFeesPending { get; set; }
    public decimal CollectionRatePercent { get; set; }
    public decimal CollectedThisMonth { get; set; }
    public decimal CollectedLastMonth { get; set; }
    public decimal MonthOnMonthChange { get; set; }

    public int TotalStudents { get; set; }
    public int PaidCount { get; set; }
    public int PartialCount { get; set; }
    public int UnpaidCount { get; set; }
    public int OverdueCount { get; set; }

    public List<ClassCollectionDto> ByClass { get; set; } = [];
    public List<DefaulterStudentDto> TopDefaulters { get; set; } = [];
    public List<DailyRevenueDto> DailyRevenue { get; set; } = [];
}

public class ClassCollectionDto
{
    public string Class { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public decimal TotalExpected { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal CollectionRatePercent { get; set; }
}

public class DefaulterStudentDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string ParentPhone { get; set; } = string.Empty;
    public decimal PendingFees { get; set; }
    public FeesStatus FeesStatus { get; set; }
    public DateTime? FeesDueDate { get; set; }
    public int DaysOverdue { get; set; }
    public string DefaulterEscalationLevel { get; set; } = "None";
    public bool NeedsPersonalFollowup { get; set; }
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Label { get; set; } = string.Empty;
}
