using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class DailySummaryService : IDailySummaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ITwilioService _twilioService;
    private readonly ILogger<DailySummaryService> _logger;

    public DailySummaryService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ITwilioService twilioService,
        ILogger<DailySummaryService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _twilioService = twilioService;
        _logger = logger;
    }

    public async Task SendAllSummariesAsync()
    {
        var schools = await _unitOfWork.Schools.FindAsync(s => s.IsActive && s.DailySummaryEnabled);

        foreach (var school in schools)
        {
            try
            {
                var summary = await BuildSummaryAsync(school.Id);

                if (!string.IsNullOrWhiteSpace(school.PrincipalEmail))
                    await _emailService.SendPrincipalDailySummaryAsync(school.PrincipalEmail, school.Name, summary);

                if (!string.IsNullOrWhiteSpace(school.PrincipalWhatsApp))
                    await SendWhatsAppSummaryAsync(school.PrincipalWhatsApp, school.Name, summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send daily summary for school {SchoolId}", school.Id);
            }
        }
    }

    private async Task<PrincipalDailySummary> BuildSummaryAsync(Guid schoolId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var callsToday = await _unitOfWork.Calls.CountAsync(
            c => c.SchoolId == schoolId && c.CreatedAt >= today && c.CreatedAt < tomorrow);

        var callsCompleted = await _unitOfWork.Calls.CountAsync(
            c => c.SchoolId == schoolId && c.CreatedAt >= today && c.CreatedAt < tomorrow
              && c.Status == CallStatus.Completed);

        var pendingStudents = await _unitOfWork.Students.FindAsync(
            s => s.SchoolId == schoolId && !s.IsDeleted
              && (s.FeesStatus == FeesStatus.Unpaid || s.FeesStatus == FeesStatus.Overdue || s.FeesStatus == FeesStatus.Partial));

        var pendingFeesCount = pendingStudents.Count();
        var pendingFeesAmount = pendingStudents.Sum(s => s.PendingFees);

        var openComplaints = await _unitOfWork.Complaints.CountAsync(
            c => c.SchoolId == schoolId
              && c.Status != ComplaintStatus.Resolved && c.Status != ComplaintStatus.Closed);

        var urgentComplaints = await _unitOfWork.Complaints.CountAsync(
            c => c.SchoolId == schoolId
              && c.Status != ComplaintStatus.Resolved && c.Status != ComplaintStatus.Closed
              && (c.Priority == ComplaintPriority.Urgent || c.Priority == ComplaintPriority.High));

        var activeCampaigns = await _unitOfWork.Campaigns.CountAsync(
            c => c.SchoolId == schoolId
              && (c.Status == CampaignStatus.Running || c.Status == CampaignStatus.Scheduled));

        return new PrincipalDailySummary
        {
            Date = today,
            CallsScheduledToday = callsToday,
            CallsCompletedToday = callsCompleted,
            PendingFeesCount = pendingFeesCount,
            PendingFeesAmount = pendingFeesAmount,
            OpenComplaintsCount = openComplaints,
            UrgentComplaintsCount = urgentComplaints,
            ActiveCampaignsCount = activeCampaigns,
        };
    }

    private async Task SendWhatsAppSummaryAsync(string phone, string schoolName, PrincipalDailySummary s)
    {
        var message = $"""
            📊 *{schoolName} — Daily Summary ({s.Date:dd MMM yyyy})*

            📞 *Calls*
            • Scheduled today: {s.CallsScheduledToday}
            • Completed: {s.CallsCompletedToday}

            💰 *Fees*
            • Students with pending fees: {s.PendingFeesCount}
            • Total pending: ₹{s.PendingFeesAmount:N0}

            ⚠️ *Complaints*
            • Open: {s.OpenComplaintsCount}
            • Urgent/High: {s.UrgentComplaintsCount}

            📣 *Campaigns*
            • Active: {s.ActiveCampaignsCount}

            _EduVoice — sent at 8 AM IST_
            """;

        await _twilioService.SendWhatsAppAsync(phone, message);
    }
}
