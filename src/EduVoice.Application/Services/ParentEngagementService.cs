using EduVoice.Application.Common;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class ParentEngagementService : IParentEngagementService
{
    private readonly IUnitOfWork _uow;
    private readonly ITwilioService _twilio;
    private readonly ILogger<ParentEngagementService> _logger;

    public ParentEngagementService(IUnitOfWork uow, ITwilioService twilio,
        ILogger<ParentEngagementService> logger)
    {
        _uow = uow;
        _twilio = twilio;
        _logger = logger;
    }

    public async Task<ApiResponse<int>> SendBirthdayWishesAsync()
    {
        var todayMonth = DateTime.UtcNow.Month;
        var todayDay = DateTime.UtcNow.Day;

        var students = await _uow.Students.Query()
            .Where(s => !s.IsDeleted && s.DateOfBirth.HasValue
                && s.DateOfBirth.Value.Month == todayMonth
                && s.DateOfBirth.Value.Day == todayDay)
            .Include(s => s.School)
            .ToListAsync();

        int sent = 0;
        foreach (var student in students)
        {
            var phone = student.ParentWhatsApp ?? student.ParentPhone;
            if (string.IsNullOrWhiteSpace(phone)) continue;

            var msg = $"""
                🎂 *Happy Birthday — {student.School.Name}*

                Dear {student.ParentName},

                Wishing *{student.FirstName} {student.LastName}* a very Happy Birthday! 🎉🌟

                May this special day bring joy, laughter, and continued success in studies.

                Warm regards,
                {student.School.Name} Family
                """;
            try
            {
                await _twilio.SendWhatsAppAsync(phone, msg);
                sent++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Birthday wish failed for student {Id}", student.Id);
            }
        }

        _logger.LogInformation("BirthdayWishes: {Count} sent", sent);
        return ApiResponse<int>.Ok(sent);
    }

    public async Task<ApiResponse<int>> SendWeeklySummariesAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-6);

        var students = await _uow.Students.Query()
            .Where(s => !s.IsDeleted
                && (s.WeeklySummarySentAt == null || s.WeeklySummarySentAt < cutoff))
            .Include(s => s.School)
            .ToListAsync();

        int sent = 0;
        foreach (var student in students)
        {
            var phone = student.ParentWhatsApp ?? student.ParentPhone;
            if (string.IsNullOrWhiteSpace(phone)) continue;

            var attendanceLine = student.AttendancePercentage.HasValue
                ? $"📅 *Attendance:* {student.AttendancePercentage:F0}%"
                    + (student.AttendancePresentDays.HasValue && student.AttendanceTotalDays.HasValue
                        ? $" ({student.AttendancePresentDays}/{student.AttendanceTotalDays} days)"
                        : "")
                : "📅 *Attendance:* Not recorded";

            var academicLine = student.Percentage.HasValue
                ? $"📚 *Academic Score:* {student.Percentage:F0}%"
                : "📚 *Academic Score:* Not recorded";

            var feesLine = student.FeesStatus switch
            {
                Domain.Enums.FeesStatus.Paid => "💰 *Fees:* ✅ Fully paid",
                Domain.Enums.FeesStatus.Partial => $"💰 *Fees:* Partial — ₹{student.PendingFees:N0} pending",
                Domain.Enums.FeesStatus.Unpaid => $"💰 *Fees:* ₹{student.PendingFees:N0} unpaid",
                Domain.Enums.FeesStatus.Overdue => $"💰 *Fees:* ⚠️ ₹{student.PendingFees:N0} OVERDUE",
                _ => ""
            };

            var msg = $"""
                📊 *Weekly Progress Update — {student.School.Name}*

                Hello {student.ParentName},

                Here is *{student.FirstName} {student.LastName}*'s weekly update (Class {student.Class ?? "—"}):

                {attendanceLine}
                {academicLine}
                {feesLine}

                For any queries, please contact the school.

                — EduVoice, {student.School.Name}
                """;
            try
            {
                await _twilio.SendWhatsAppAsync(phone, msg);
                student.WeeklySummarySentAt = DateTime.UtcNow;
                await _uow.Students.UpdateAsync(student);
                sent++;
                await Task.Delay(1100); // 1.1s throttle for Twilio
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Weekly summary failed for student {Id}", student.Id);
            }
        }

        if (sent > 0)
            await _uow.SaveChangesAsync();

        _logger.LogInformation("WeeklySummary: {Count} sent", sent);
        return ApiResponse<int>.Ok(sent);
    }

    public async Task CheckAchievementAlertAsync(Student student, School school)
    {
        if (student.Percentage is null || student.Percentage < school.AchievementThreshold)
            return;

        // 30-day cooldown to avoid repeated alerts for the same student
        if (student.AchievementAlertSentAt.HasValue
            && (DateTime.UtcNow - student.AchievementAlertSentAt.Value).TotalDays < 30)
            return;

        var phone = student.ParentWhatsApp ?? student.ParentPhone;
        if (string.IsNullOrWhiteSpace(phone)) return;

        var msg = $"""
            🌟 *Achievement Alert — {school.Name}*

            Congratulations, {student.ParentName}!

            Your child *{student.FirstName} {student.LastName}* (Class {student.Class ?? "—"}) has achieved an excellent score of *{student.Percentage:F0}%* in their recent exam! 🏆

            Keep up the great work — we're proud of this achievement!

            — EduVoice, {school.Name}
            """;

        try
        {
            await _twilio.SendWhatsAppAsync(phone, msg);
            student.AchievementAlertSentAt = DateTime.UtcNow;
            await _uow.Students.UpdateAsync(student);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Achievement alert failed for student {Id}", student.Id);
        }
    }
}
