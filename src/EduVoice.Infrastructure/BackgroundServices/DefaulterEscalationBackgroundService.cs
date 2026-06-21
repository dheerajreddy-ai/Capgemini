using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.BackgroundServices;

/// <summary>
/// Daily 7AM IST: checks overdue students and escalates call cadence.
/// 30d overdue → Day30 (flag for extra attention)
/// 60d overdue → Day60 (3x/week call cadence marker)
/// 90d overdue → Day90 + NeedsPersonalFollowup + WhatsApp alert to principal
/// </summary>
public class DefaulterEscalationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DefaulterEscalationBackgroundService> _logger;

    private static readonly TimeZoneInfo Ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    public DefaulterEscalationBackgroundService(IServiceScopeFactory scopeFactory,
        ILogger<DefaulterEscalationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);

            if (nowIst.Hour == 7 && nowIst.Minute < 5)
            {
                try { await RunAsync(); }
                catch (Exception ex) { _logger.LogError(ex, "DefaulterEscalationBackgroundService failed"); }
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task RunAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var twilioService = scope.ServiceProvider.GetRequiredService<ITwilioService>();

        var today = DateTime.UtcNow.Date;

        var overdueStudents = await uow.Students.Query()
            .Where(s => !s.IsDeleted && !s.IsScholarship && !s.HasFeeDispute
                && (s.FeesStatus == FeesStatus.Overdue || s.FeesStatus == FeesStatus.Unpaid)
                && s.FeesDueDate.HasValue && s.PendingFees > 0)
            .Include(s => s.School)
            .ToListAsync();

        int escalated = 0;
        var newFollowups = new Dictionary<Guid, List<string>>();  // schoolId → student names

        foreach (var student in overdueStudents)
        {
            var daysOverdue = (today - student.FeesDueDate!.Value.Date).Days;
            if (daysOverdue <= 0) continue;

            var newLevel = daysOverdue switch
            {
                >= 90 => DefaulterEscalationLevel.Day90,
                >= 60 => DefaulterEscalationLevel.Day60,
                >= 30 => DefaulterEscalationLevel.Day30,
                _ => DefaulterEscalationLevel.None
            };

            if (newLevel == DefaulterEscalationLevel.None) continue;
            if (student.DefaulterEscalationLevel == newLevel) continue; // already at this level

            student.DefaulterEscalationLevel = newLevel;
            student.DefaulterEscalatedAt = DateTime.UtcNow;

            if (newLevel == DefaulterEscalationLevel.Day90 && !student.NeedsPersonalFollowup)
            {
                student.NeedsPersonalFollowup = true;

                if (!newFollowups.ContainsKey(student.SchoolId))
                    newFollowups[student.SchoolId] = [];
                newFollowups[student.SchoolId].Add(
                    $"• {student.FirstName} {student.LastName} (Class {student.Class}) — ₹{student.PendingFees:N0} pending {daysOverdue} days");
            }

            student.UpdatedAt = DateTime.UtcNow;
            await uow.Students.UpdateAsync(student);
            escalated++;
        }

        if (escalated > 0)
            await uow.SaveChangesAsync();

        // Send WhatsApp alert to principal for new 90-day escalations
        foreach (var (schoolId, names) in newFollowups)
        {
            var school = await uow.Schools.GetByIdAsync(schoolId);
            if (school is null || string.IsNullOrWhiteSpace(school.ContactPhone)) continue;

            var msg = $"""
                🚨 *EduVoice Fee Alert — {school.Name}*

                The following students have crossed *90 days overdue* and need personal follow-up:

                {string.Join("\n", names)}

                Please assign a staff member to call these families directly.
                """;
            try { await twilioService.SendWhatsAppAsync(school.ContactPhone, msg); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to send 90d alert for school {Id}", schoolId); }
        }

        _logger.LogInformation("DefaulterEscalation: {Count} students escalated", escalated);
    }
}
