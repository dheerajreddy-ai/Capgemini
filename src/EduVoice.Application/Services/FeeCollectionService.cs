using EduVoice.Application.Common;
using EduVoice.Application.DTOs.FeeCollection;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class FeeCollectionService : IFeeCollectionService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<FeeCollectionService> _logger;

    public FeeCollectionService(IUnitOfWork uow, ILogger<FeeCollectionService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<ApiResponse<FeeCollectionDashboardDto>> GetDashboardAsync(Guid schoolId)
    {
        try
        {
            var students = await _uow.Students.Query()
                .Where(s => s.SchoolId == schoolId && !s.IsDeleted)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var startOf30DaysAgo = today.AddDays(-29);

            // Aggregate payments from Call records (FeesConfirmed calls as proxy for payment events)
            // Use LastPaymentDate on Student as the payment date signal
            var thisMonthPaid = students
                .Where(s => s.LastPaymentDate.HasValue && s.LastPaymentDate.Value.Date >= startOfMonth)
                .Sum(s => s.PaidFees);

            var lastMonthPaid = students
                .Where(s => s.LastPaymentDate.HasValue
                    && s.LastPaymentDate.Value.Date >= startOfLastMonth
                    && s.LastPaymentDate.Value.Date < startOfMonth)
                .Sum(s => s.PaidFees);

            var totalExpected = students.Sum(s => s.TotalFees);
            var totalCollected = students.Sum(s => s.PaidFees);
            var totalPending = students.Sum(s => s.PendingFees);
            var collectionRate = totalExpected > 0 ? Math.Round(totalCollected / totalExpected * 100, 1) : 0;
            var mom = lastMonthPaid > 0 ? Math.Round((thisMonthPaid - lastMonthPaid) / lastMonthPaid * 100, 1) : 0;

            // By class
            var byClass = students
                .Where(s => s.Class != null)
                .GroupBy(s => s.Class!)
                .Select(g =>
                {
                    var exp = g.Sum(s => s.TotalFees);
                    var col = g.Sum(s => s.PaidFees);
                    return new ClassCollectionDto
                    {
                        Class = g.Key,
                        StudentCount = g.Count(),
                        TotalExpected = exp,
                        TotalCollected = col,
                        CollectionRatePercent = exp > 0 ? Math.Round(col / exp * 100, 1) : 0
                    };
                })
                .OrderBy(c => c.Class)
                .ToList();

            // Top 10 defaulters (overdue/unpaid, highest pending first, skip scholarship)
            var topDefaulters = students
                .Where(s => !s.IsScholarship && s.PendingFees > 0
                    && (s.FeesStatus == FeesStatus.Overdue || s.FeesStatus == FeesStatus.Unpaid))
                .OrderByDescending(s => s.PendingFees)
                .Take(10)
                .Select(s => new DefaulterStudentDto
                {
                    StudentId = s.Id,
                    StudentName = $"{s.FirstName} {s.LastName}",
                    Class = s.Class,
                    Section = s.Section,
                    ParentPhone = s.ParentPhone,
                    PendingFees = s.PendingFees,
                    FeesStatus = s.FeesStatus,
                    FeesDueDate = s.FeesDueDate,
                    DaysOverdue = s.FeesDueDate.HasValue
                        ? Math.Max(0, (today - s.FeesDueDate.Value.Date).Days) : 0,
                    DefaulterEscalationLevel = s.DefaulterEscalationLevel.ToString(),
                    NeedsPersonalFollowup = s.NeedsPersonalFollowup
                })
                .ToList();

            // Daily revenue for last 30 days — approximation using students with recent payment dates
            var dailyRevenue = Enumerable.Range(0, 30)
                .Select(i => today.AddDays(-29 + i))
                .Select(date => new DailyRevenueDto
                {
                    Date = date,
                    Label = date.ToString("dd MMM"),
                    Amount = students
                        .Where(s => s.LastPaymentDate.HasValue && s.LastPaymentDate.Value.Date == date)
                        .Sum(s => s.PaidFees)
                })
                .ToList();

            return ApiResponse<FeeCollectionDashboardDto>.Ok(new FeeCollectionDashboardDto
            {
                TotalFeesExpected = totalExpected,
                TotalFeesCollected = totalCollected,
                TotalFeesPending = totalPending,
                CollectionRatePercent = collectionRate,
                CollectedThisMonth = thisMonthPaid,
                CollectedLastMonth = lastMonthPaid,
                MonthOnMonthChange = mom,
                TotalStudents = students.Count,
                PaidCount = students.Count(s => s.FeesStatus == FeesStatus.Paid),
                PartialCount = students.Count(s => s.FeesStatus == FeesStatus.Partial),
                UnpaidCount = students.Count(s => s.FeesStatus == FeesStatus.Unpaid),
                OverdueCount = students.Count(s => s.FeesStatus == FeesStatus.Overdue),
                ByClass = byClass,
                TopDefaulters = topDefaulters,
                DailyRevenue = dailyRevenue
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building fee collection dashboard for school {SchoolId}", schoolId);
            return ApiResponse<FeeCollectionDashboardDto>.Fail("Failed to build dashboard", "FETCH_ERROR");
        }
    }
}
