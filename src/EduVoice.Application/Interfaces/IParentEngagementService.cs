using EduVoice.Application.Common;
using EduVoice.Domain.Entities;

namespace EduVoice.Application.Interfaces;

public interface IParentEngagementService
{
    Task<ApiResponse<int>> SendBirthdayWishesAsync();
    Task<ApiResponse<int>> SendWeeklySummariesAsync();
    Task CheckAchievementAlertAsync(Student student, School school);
}
