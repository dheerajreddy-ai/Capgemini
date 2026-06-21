namespace EduVoice.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TodaysCalls { get; set; }
    public int WeeksCalls { get; set; }
    public int FeesConfirmedToday { get; set; }
    public int ComplaintsNew { get; set; }
    public int PendingFollowups { get; set; }
    public List<DailyCallCountDto> CallsThisWeek { get; set; } = new();
    public SentimentBreakdownDto SentimentBreakdown { get; set; } = new();
    public List<CampaignSummaryDto> CampaignsSummary { get; set; } = new();
    public int TotalStudents { get; set; }
    public int ActiveCampaigns { get; set; }
}

public class DailyCallCountDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public string DayLabel { get; set; } = string.Empty;
}

public class SentimentBreakdownDto
{
    public int Positive { get; set; }
    public int Neutral { get; set; }
    public int Negative { get; set; }
    public int Angry { get; set; }
}

public class CampaignSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int CallsCompleted { get; set; }
    public int CallsFailed { get; set; }
    public double ProgressPercent { get; set; }
}
