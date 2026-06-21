namespace EduVoice.Application.DTOs.Instalments;

public class FeeInstalmentDto
{
    public Guid Id { get; set; }
    public int InstalmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }
}

public class InstalmentPlanDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public decimal TotalPending { get; set; }
    public int TotalInstalments { get; set; }
    public int PaidInstalments { get; set; }
    public int OverdueInstalments { get; set; }
    public List<FeeInstalmentDto> Instalments { get; set; } = [];
}

public record CreateInstalmentPlanRequest(
    int InstalmentCount,
    DateTime FirstDueDate,
    int IntervalDays
);
