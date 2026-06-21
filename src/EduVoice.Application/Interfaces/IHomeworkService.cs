using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Homework;

namespace EduVoice.Application.Interfaces;

public interface IHomeworkService
{
    Task<ApiResponse<PagedResult<HomeworkDto>>> GetAllAsync(Guid schoolId, int page, int pageSize, DateTime? date);
    Task<ApiResponse<HomeworkDto>> GetByIdAsync(Guid schoolId, Guid id);
    Task<ApiResponse<HomeworkDto>> CreateAsync(Guid schoolId, Guid userId, CreateHomeworkRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid schoolId, Guid id);
    Task SendDailyAlertsAsync();
}
