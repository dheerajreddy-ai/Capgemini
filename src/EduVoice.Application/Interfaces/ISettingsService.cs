using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Settings;

namespace EduVoice.Application.Interfaces;

public interface ISettingsService
{
    Task<ApiResponse<SchoolSettingsDto>> GetSettingsAsync(Guid schoolId);
    Task<ApiResponse<SchoolSettingsDto>> UpdateSettingsAsync(Guid schoolId, UpdateSettingsRequest request);
    Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
