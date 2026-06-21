using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Dashboard;

namespace EduVoice.Application.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardStatsDto>> GetStatsAsync(Guid schoolId);
}
