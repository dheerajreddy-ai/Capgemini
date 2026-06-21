using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Instalments;

namespace EduVoice.Application.Interfaces;

public interface IInstalmentService
{
    Task<ApiResponse<InstalmentPlanDto>> CreatePlanAsync(Guid schoolId, Guid studentId, CreateInstalmentPlanRequest request);
    Task<ApiResponse<InstalmentPlanDto>> GetPlanAsync(Guid schoolId, Guid studentId);
    Task<ApiResponse<FeeInstalmentDto>> MarkPaidAsync(Guid schoolId, Guid instalmentId);
    Task<ApiResponse> DeletePlanAsync(Guid schoolId, Guid studentId);
    Task CheckOverdueAndRemindAsync();
}
