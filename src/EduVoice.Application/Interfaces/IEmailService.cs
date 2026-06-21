using EduVoice.Application.DTOs.Dashboard;
using EduVoice.Domain.Entities;

namespace EduVoice.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken, string schoolName);
    Task SendDailySummaryAsync(string email, DashboardStatsDto stats);
    Task SendComplaintAlertAsync(string email, Complaint complaint);
}
