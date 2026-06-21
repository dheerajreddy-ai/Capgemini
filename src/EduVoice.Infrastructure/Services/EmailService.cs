using EduVoice.Application.DTOs.Dashboard;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EduVoice.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken, string schoolName)
    {
        try
        {
            var frontendUrl = _configuration["FRONTEND_URL"] ?? "http://localhost:4200";
            var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(email)}";

            var subject = $"Password Reset - {schoolName} EduVoice";
            var htmlContent = $"""
                <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                    <h2>Password Reset Request</h2>
                    <p>You requested a password reset for your EduVoice account at {schoolName}.</p>
                    <p>Click the link below to reset your password. This link expires in 1 hour.</p>
                    <a href="{resetLink}" style="background-color: #4F46E5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; margin: 16px 0;">
                        Reset Password
                    </a>
                    <p>If you did not request this, please ignore this email.</p>
                    <p>The EduVoice Team</p>
                </div>
                """;

            await SendEmailAsync(email, subject, htmlContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
        }
    }

    public async Task SendDailySummaryAsync(string email, DashboardStatsDto stats)
    {
        try
        {
            var subject = $"EduVoice Daily Summary - {DateTime.UtcNow:dd MMM yyyy}";
            var htmlContent = $"""
                <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                    <h2>Daily Summary Report</h2>
                    <table style="width:100%; border-collapse: collapse;">
                        <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>Today's Calls</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{stats.TodaysCalls}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>Fees Confirmed Today</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{stats.FeesConfirmedToday}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>New Complaints</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{stats.ComplaintsNew}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>Pending Followups</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{stats.PendingFollowups}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>This Week's Calls</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{stats.WeeksCalls}</td></tr>
                    </table>
                    <p>The EduVoice Team</p>
                </div>
                """;

            await SendEmailAsync(email, subject, htmlContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send daily summary to {Email}", email);
        }
    }

    public async Task SendComplaintAlertAsync(string email, Complaint complaint)
    {
        try
        {
            var subject = $"New Complaint Alert - Priority: {complaint.Priority}";
            var htmlContent = $"""
                <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                    <h2 style="color: {(complaint.Priority.ToString() == "Urgent" ? "#DC2626" : "#F59E0B")};">
                        New Complaint - {complaint.Priority} Priority
                    </h2>
                    <table style="width:100%; border-collapse: collapse;">
                        <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>Category</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{complaint.Category}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>Parent</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{complaint.ParentName} ({complaint.ParentPhone})</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>Summary</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{complaint.Summary}</td></tr>
                        <tr><td style="padding: 8px; border: 1px solid #ddd;"><strong>Received</strong></td><td style="padding: 8px; border: 1px solid #ddd;">{complaint.CreatedAt:dd MMM yyyy HH:mm} UTC</td></tr>
                    </table>
                    <p>Please login to EduVoice to handle this complaint.</p>
                    <p>The EduVoice Team</p>
                </div>
                """;

            await SendEmailAsync(email, subject, htmlContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send complaint alert to {Email}", email);
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var apiKey = _configuration["SENDGRID_API_KEY"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("SENDGRID_API_KEY not configured. Email not sent to {Email}", toEmail);
            return;
        }

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("noreply@eduvoice.in", "EduVoice");
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);

        var response = await client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
            _logger.LogWarning("SendGrid returned {StatusCode} for email to {Email}", response.StatusCode, toEmail);
    }
}
