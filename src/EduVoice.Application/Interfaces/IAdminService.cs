using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Admin;
using EduVoice.Application.DTOs.Dashboard;

namespace EduVoice.Application.Interfaces;

public interface IAdminService
{
    Task<ApiResponse<PagedResult<AdminSchoolDto>>> GetSchoolsAsync(int page, int pageSize, string? search);
    Task<ApiResponse<AdminSchoolDto>> CreateSchoolAsync(CreateSchoolRequest request);
    Task<ApiResponse<AdminSchoolDto>> UpdateSchoolAsync(Guid schoolId, CreateSchoolRequest request);
    Task<ApiResponse> DeleteSchoolAsync(Guid schoolId);
    Task<ApiResponse<DashboardStatsDto>> GetSchoolStatsAsync(Guid schoolId);
}
