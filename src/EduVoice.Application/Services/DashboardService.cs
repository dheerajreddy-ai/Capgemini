using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Dashboard;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetStatsAsync(Guid schoolId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-6);

            var allCalls = _unitOfWork.Calls.Query()
                .Where(c => c.SchoolId == schoolId);

            var todaysCalls = await allCalls
                .Where(c => c.CreatedAt >= today)
                .CountAsync();

            var weeksCalls = await allCalls
                .Where(c => c.CreatedAt >= weekStart)
                .CountAsync();

            var feesConfirmedToday = await allCalls
                .Where(c => c.CreatedAt >= today && c.FeesConfirmed)
                .CountAsync();

            var complaintsNew = await _unitOfWork.Complaints.Query()
                .Where(c => c.SchoolId == schoolId && c.Status == ComplaintStatus.New)
                .CountAsync();

            var pendingFollowups = await allCalls
                .Where(c => c.CallbackRequested && c.Status == CallStatus.Completed)
                .CountAsync();

            var callsThisWeek = new List<DailyCallCountDto>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var count = await allCalls
                    .Where(c => c.CreatedAt >= date && c.CreatedAt < date.AddDays(1))
                    .CountAsync();
                callsThisWeek.Add(new DailyCallCountDto
                {
                    Date = date,
                    Count = count,
                    DayLabel = date.ToString("ddd")
                });
            }

            var completedCalls = await allCalls
                .Where(c => c.Status == CallStatus.Completed && c.Sentiment.HasValue)
                .ToListAsync();

            var sentimentBreakdown = new SentimentBreakdownDto
            {
                Positive = completedCalls.Count(c => c.Sentiment == SentimentType.Positive),
                Neutral = completedCalls.Count(c => c.Sentiment == SentimentType.Neutral),
                Negative = completedCalls.Count(c => c.Sentiment == SentimentType.Negative),
                Angry = completedCalls.Count(c => c.Sentiment == SentimentType.Angry)
            };

            var recentCampaigns = await _unitOfWork.Campaigns.Query()
                .Where(c => c.SchoolId == schoolId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();

            var campaignsSummary = recentCampaigns.Select(c => new CampaignSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Status = c.Status.ToString(),
                TotalStudents = c.TotalStudents,
                CallsCompleted = c.CallsCompleted,
                CallsFailed = c.CallsFailed,
                ProgressPercent = c.TotalStudents > 0
                    ? Math.Round((double)(c.CallsCompleted + c.CallsFailed) / c.TotalStudents * 100, 1)
                    : 0
            }).ToList();

            var totalStudents = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && !s.IsDeleted)
                .CountAsync();

            var activeCampaigns = await _unitOfWork.Campaigns.Query()
                .Where(c => c.SchoolId == schoolId &&
                    (c.Status == CampaignStatus.Running || c.Status == CampaignStatus.Paused))
                .CountAsync();

            return ApiResponse<DashboardStatsDto>.Ok(new DashboardStatsDto
            {
                TodaysCalls = todaysCalls,
                WeeksCalls = weeksCalls,
                FeesConfirmedToday = feesConfirmedToday,
                ComplaintsNew = complaintsNew,
                PendingFollowups = pendingFollowups,
                CallsThisWeek = callsThisWeek,
                SentimentBreakdown = sentimentBreakdown,
                CampaignsSummary = campaignsSummary,
                TotalStudents = totalStudents,
                ActiveCampaigns = activeCampaigns
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard stats for school {SchoolId}", schoolId);
            return ApiResponse<DashboardStatsDto>.Fail("Failed to fetch dashboard stats", "STATS_ERROR");
        }
    }
}
