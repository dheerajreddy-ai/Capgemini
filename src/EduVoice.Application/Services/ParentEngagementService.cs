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
                ðŸŽ‚ *Happy Birthday â€” {student.School.Name}*

                Dear {student.ParentName},

                Wishing *{student.FirstName} {student.LastName}* a very Happy Birthday! ðŸŽ‰ðŸŒŸ

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
                ? $"ðŸ“… *Attendance:* {student.AttendancePercentage:F0}%"
                    + (student.AttendancePresentDays.HasValue && student.AttendanceTotalDays.HasValue
                        ? $" ({student.AttendancePresentDays}/{student.AttendanceTotalDays} days)"
                        : "")
                : "ðŸ“… *Attendance:* Not recorded";

            var academicLine = student.Percentage.HasValue
                ? $"ðŸ“š *Academic Score:* {student.Percentage:F0}%"
                : "ðŸ“š *Academic Score:* Not recorded";

            var feesLine = student.FeesStatus switch
            {
                Domain.Enums.FeesStatus.Paid => "ðŸ’° *Fees:* âœ… Fully paid",
                Domain.Enums.FeesStatus.Partial => $"ðŸ’° *Fees:* Partial â€” â‚¹{student.PendingFees:N0} pending",
                Domain.Enums.FeesStatus.Unpaid => $"ðŸ’° *Fees:* â‚¹{student.PendingFees:N0} unpaid",
                Domain.Enums.FeesStatus.Overdue => $"ðŸ’° *Fees:* âš ï¸ â‚¹{student.PendingFees:N0} OVERDUE",
                _ => ""
            };

            var msg = $"""
                ðŸ“Š *Weekly Progress Update â€” {student.School.Name}*

                Hello {student.ParentName},

                Here is *{student.FirstName} {student.LastName}*'s weekly update (Class {student.Class ?? "â€”"}):

                {attendanceLine}
                {academicLine}
                {feesLine}

                For any queries, please contact the school.

                â€” EduVoice, {student.School.Name}
                """;
            try
            {
                await _twilio.SendWhatsAppAsync(phone, msg);
                student.WeeklySummarySentAt = DateTime.UtcNow;
                _uow.Students.Update(student);
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
            ðŸŒŸ *Achievement Alert â€” {school.Name}*

            Congratulations, {student.ParentName}!

            Your child *{student.FirstName} {student.LastName}* (Class {student.Class ?? "â€”"}) has achieved an excellent score of *{student.Percentage:F0}%* in their recent exam! ðŸ†

            Keep up the great work â€” we're proud of this achievement!

            â€” EduVoice, {school.Name}
            """;

        try
        {
            await _twilio.SendWhatsAppAsync(phone, msg);
            student.AchievementAlertSentAt = DateTime.UtcNow;
            _uow.Students.Update(student);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Achievement alert failed for student {Id}", student.Id);
        }
    }
}
