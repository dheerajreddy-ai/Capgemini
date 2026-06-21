using EduVoice.Application.Common;
using EduVoice.Application.DTOs.ComplaintSla;
using EduVoice.Domain.Enums;

namespace EduVoice.Application.Interfaces;

public interface IComplaintSlaService
{
    Task<ApiResponse<List<ComplaintSlaConfigDto>>> GetConfigsAsync(Guid schoolId);
    Task<ApiResponse<ComplaintSlaConfigDto>> UpsertConfigAsync(Guid schoolId, UpsertSlaConfigRequest request);
    Task CheckAndEscalateAsync();
}
