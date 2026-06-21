using EduVoice.Application.Common;
using EduVoice.Application.DTOs.StaffOperations;

namespace EduVoice.Application.Interfaces;

public interface IStaffOperationsService
{
    Task<ApiResponse<StaffAbsenceDto>> LogAbsenceAsync(Guid schoolId, Guid userId, LogAbsenceRequest request);
    Task<ApiResponse<List<StaffAbsenceDto>>> GetAbsencesAsync(Guid schoolId, DateTime? date = null);
    Task<ApiResponse<PtmScheduleDto>> SchedulePtmAsync(Guid schoolId, Guid userId, CreatePtmRequest request);
    Task<ApiResponse<List<PtmScheduleDto>>> GetPtmSchedulesAsync(Guid schoolId);
    Task<ApiResponse> DeletePtmAsync(Guid schoolId, Guid ptmId);
    Task ProcessPtmRemindersAsync();
}
