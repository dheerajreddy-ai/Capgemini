using EduVoice.Application.Common;
using EduVoice.Application.DTOs.ExamSchedules;

namespace EduVoice.Application.Interfaces;

public interface IExamScheduleService
{
    Task<ApiResponse<PagedResult<ExamScheduleDto>>> GetAllAsync(Guid schoolId, int page, int pageSize);
    Task<ApiResponse<ExamScheduleDto>> GetByIdAsync(Guid schoolId, Guid id);
    Task<ApiResponse<ExamScheduleDto>> CreateAsync(Guid schoolId, Guid userId, CreateExamScheduleRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid schoolId, Guid id);
    Task ProcessRemindersAsync();
}
