using EduVoice.Application.DTOs.Dashboard;
using EduVoice.Domain.Entities;

namespace EduVoice.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken, string schoolName);
    Task SendDailySummaryAsync(string email, DashboardStatsDto stats);
    Task SendComplaintAlertAsync(string email, Complaint complaint);
    Task SendSlaEscalationEmailAsync(string email, Complaint complaint);
    Task SendGenericEmailAsync(string email, string subject, string htmlBody);
    Task SendPrincipalDailySummaryAsync(string email, string schoolName, PrincipalDailySummary summary);
}

public class PrincipalDailySummary
{
    public int CallsScheduledToday { get; set; }
    public int CallsCompletedToday { get; set; }
    public int PendingFeesCount { get; set; }
    public decimal PendingFeesAmount { get; set; }
    public int OpenComplaintsCount { get; set; }
    public int UrgentComplaintsCount { get; set; }
    public int ActiveCampaignsCount { get; set; }
    public DateTime Date { get; set; }
}
