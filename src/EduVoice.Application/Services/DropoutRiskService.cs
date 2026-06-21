using EduVoice.Application.Common;
using EduVoice.Application.DTOs.DropoutRisk;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class DropoutRiskService : IDropoutRiskService
{
    private readonly IUnitOfWork _uow;
    private readonly ITwilioService _twilioService;
    private readonly ILogger<DropoutRiskService> _logger;

    public DropoutRiskService(IUnitOfWork uow, ITwilioService twilioService, ILogger<DropoutRiskService> logger)
    {
        _uow = uow;
        _twilioService = twilioService;
        _logger = logger;
    }

    public async Task<ApiResponse<DropoutRiskSummaryDto>> GetRiskSummaryAsync(Guid schoolId, string? riskLevel = null)
    {
        try
        {
            var query = _uow.Students.Query().Where(s => s.SchoolId == schoolId && !s.IsDeleted);

            if (!string.IsNullOrEmpty(riskLevel) && Enum.TryParse<DropoutRiskLevel>(riskLevel, out var level))
                query = query.Where(s => s.DropoutRiskLevel == level);

            var students = await query.OrderByDescending(s => s.DropoutRiskScore).ToListAsync();

            var cutoff30 = DateTime.UtcNow.AddDays(-30);
            var noAnswerCounts = await _uow.Calls.Query()
                .Where(c => c.SchoolId == schoolId && c.CreatedAt >= cutoff30 && c.Status == CallStatus.NoAnswer)
                .GroupBy(c => c.StudentId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            var atRisk = students
                .Where(s => s.DropoutRiskLevel != DropoutRiskLevel.Low)
                .Select(s => new DropoutRiskStudentDto
                {
                    StudentId = s.Id,
                    StudentName = $"{s.FirstName} {s.LastName}",
                    Class = s.Class,
                    Section = s.Section,
                    ParentPhone = s.ParentPhone,
                    RiskScore = s.DropoutRiskScore,
                    RiskLevel = s.DropoutRiskLevel,
                    RiskReasons = string.IsNullOrEmpty(s.DropoutRiskReasons)
                        ? [] : [.. s.DropoutRiskReasons.Split('|')],
                    AttendancePercentage = s.AttendancePercentage,
                    AcademicPercentage = s.Percentage,
                    FeesStatus = s.FeesStatus,
                    PendingFees = s.PendingFees,
                    NoAnswerCallsLast30Days = noAnswerCounts.GetValueOrDefault(s.Id, 0),
                    CalculatedAt = s.DropoutRiskCalculatedAt
                })
                .ToList();

            var summary = new DropoutRiskSummaryDto
            {
                TotalStudents = students.Count,
                CriticalCount = students.Count(s => s.DropoutRiskLevel == DropoutRiskLevel.Critical),
                HighCount = students.Count(s => s.DropoutRiskLevel == DropoutRiskLevel.High),
                MediumCount = students.Count(s => s.DropoutRiskLevel == DropoutRiskLevel.Medium),
                LowCount = students.Count(s => s.DropoutRiskLevel == DropoutRiskLevel.Low),
                AtRiskStudents = atRisk
            };

            return ApiResponse<DropoutRiskSummaryDto>.Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dropout risk summary for school {SchoolId}", schoolId);
            return ApiResponse<DropoutRiskSummaryDto>.Fail("Failed to fetch risk summary", "FETCH_ERROR");
        }
    }

    public async Task RecalculateAllAsync()
    {
        var schools = await _uow.Schools.Query().Where(s => s.IsActive).ToListAsync();

        foreach (var school in schools)
        {
            try
            {
                var students = await _uow.Students.Query()
                    .Where(s => s.SchoolId == school.Id && !s.IsDeleted)
                    .ToListAsync();

                var cutoff30 = DateTime.UtcNow.AddDays(-30);
                var noAnswerCounts = await _uow.Calls.Query()
                    .Where(c => c.SchoolId == school.Id && c.CreatedAt >= cutoff30 && c.Status == CallStatus.NoAnswer)
                    .GroupBy(c => c.StudentId)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Key, x => x.Count);

                var criticalStudents = new List<string>();

                foreach (var student in students)
                {
                    var reasons = new List<string>();
                    int score = 0;

                    // Attendance risk (max 30 pts)
                    if (student.AttendancePercentage.HasValue)
                    {
                        if (student.AttendancePercentage < 50) { score += 30; reasons.Add("Attendance below 50%"); }
                        else if (student.AttendancePercentage < 65) { score += 20; reasons.Add("Attendance below 65%"); }
                        else if (student.AttendancePercentage < 75) { score += 10; reasons.Add("Attendance below 75%"); }
                    }

                    // Academic risk (max 25 pts)
                    if (student.Percentage.HasValue)
                    {
                        if (student.Percentage < 30) { score += 25; reasons.Add("Academic score below 30%"); }
                        else if (student.Percentage < 40) { score += 15; reasons.Add("Academic score below 40%"); }
                        else if (student.Percentage < 50) { score += 8; reasons.Add("Academic score below 50%"); }
                    }

                    // Fee risk (max 25 pts)
                    if (student.FeesStatus == FeesStatus.Overdue) { score += 25; reasons.Add("Fees overdue"); }
                    else if (student.FeesStatus == FeesStatus.Unpaid && student.PendingFees > 0) { score += 15; reasons.Add("Fees unpaid"); }
                    else if (student.FeesStatus == FeesStatus.Partial) { score += 5; reasons.Add("Partial fee payment"); }

                    // Parent disengagement risk (max 20 pts)
                    var noAnswers = noAnswerCounts.GetValueOrDefault(student.Id, 0);
                    if (noAnswers >= 5) { score += 20; reasons.Add($"{noAnswers} unanswered calls in 30 days"); }
                    else if (noAnswers >= 3) { score += 10; reasons.Add($"{noAnswers} unanswered calls in 30 days"); }

                    student.DropoutRiskScore = Math.Min(score, 100);
                    student.DropoutRiskLevel = score switch
                    {
                        >= 70 => DropoutRiskLevel.Critical,
                        >= 45 => DropoutRiskLevel.High,
                        >= 20 => DropoutRiskLevel.Medium,
                        _ => DropoutRiskLevel.Low
                    };
                    student.DropoutRiskReasons = reasons.Count > 0 ? string.Join("|", reasons) : null;
                    student.DropoutRiskCalculatedAt = DateTime.UtcNow;
                    student.UpdatedAt = DateTime.UtcNow;
                    _uow.Students.Update(student);

                    if (student.DropoutRiskLevel == DropoutRiskLevel.Critical)
                        criticalStudents.Add($"â€¢ {student.FirstName} {student.LastName} (Class {student.Class}) â€” Score {student.DropoutRiskScore}/100");
                }

                await _uow.SaveChangesAsync();

                // Alert principal via WhatsApp if there are critical students
                if (criticalStudents.Count > 0 && !string.IsNullOrWhiteSpace(school.ContactPhone))
                {
                    var alert = $"""
                        ðŸš¨ *EduVoice Dropout Risk Alert â€” {school.Name}*

                        The following students are at *Critical* dropout risk today:

                        {string.Join("\n", criticalStudents)}

                        Please review and intervene urgently.
                        View full report: {school.SubDomain}.eduvoice.app/analytics
                        """;

                    try { await _twilioService.SendWhatsAppAsync(school.ContactPhone, alert); }
                    catch (Exception ex) { _logger.LogWarning(ex, "Failed to send critical risk alert for school {Id}", school.Id); }
                }

                _logger.LogInformation("Dropout risk recalculated for school {Id}: {Count} students", school.Id, students.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating dropout risk for school {Id}", school.Id);
            }
        }
    }
}
