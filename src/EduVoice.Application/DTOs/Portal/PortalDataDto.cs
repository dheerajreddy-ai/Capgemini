namespace EduVoice.Application.DTOs.Portal;

public class PortalDataDto
{
    public string StudentName { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string FeesStatus { get; set; } = string.Empty;
    public decimal PendingFees { get; set; }
    public decimal TotalFees { get; set; }
    public decimal PaidFees { get; set; }
    public DateTime? FeesDueDate { get; set; }
    public string? PaymentLink { get; set; }
    public decimal? AttendancePercentage { get; set; }
    public decimal? Percentage { get; set; }
    public string? Grade { get; set; }
    public decimal? MathMarks { get; set; }
    public decimal? ScienceMarks { get; set; }
    public decimal? EnglishMarks { get; set; }
    public decimal? TeluguMarks { get; set; }
    public decimal? SocialMarks { get; set; }
    public List<PortalCallDto> RecentCalls { get; set; } = [];
    public List<PortalComplaintDto> OpenComplaints { get; set; } = [];
}

public class PortalCallDto
{
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AiSummary { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PortalComplaintDto
{
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
