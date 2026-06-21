using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Settings;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class SettingsService : ISettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(IUnitOfWork unitOfWork, IAuditService auditService, ILogger<SettingsService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ApiResponse<SchoolSettingsDto>> GetSettingsAsync(Guid schoolId)
    {
        try
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null)
                return ApiResponse<SchoolSettingsDto>.Fail("School not found", "NOT_FOUND");

            return ApiResponse<SchoolSettingsDto>.Ok(new SchoolSettingsDto
            {
                Id = school.Id,
                Name = school.Name,
                LogoUrl = school.LogoUrl,
                SubDomain = school.SubDomain,
                PrimaryColor = school.PrimaryColor,
                ContactEmail = school.ContactEmail,
                ContactPhone = school.ContactPhone,
                Address = school.Address,
                City = school.City,
                State = school.State,
                PlanType = school.PlanType,
                TwilioPhoneNumber = school.TwilioPhoneNumber,
                VapiAssistantId = school.VapiAssistantId,
                ElevenLabsVoiceId = school.ElevenLabsVoiceId,
                IsActive = school.IsActive,
                TrialEndsAt = school.TrialEndsAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching settings for school {SchoolId}", schoolId);
            return ApiResponse<SchoolSettingsDto>.Fail("Failed to fetch settings", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<SchoolSettingsDto>> UpdateSettingsAsync(Guid schoolId, UpdateSettingsRequest request)
    {
        try
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null)
                return ApiResponse<SchoolSettingsDto>.Fail("School not found", "NOT_FOUND");

            school.Name = request.Name;
            school.LogoUrl = request.LogoUrl;
            school.PrimaryColor = request.PrimaryColor;
            school.ContactEmail = request.ContactEmail;
            school.ContactPhone = request.ContactPhone;
            school.Address = request.Address;
            school.City = request.City;
            school.State = request.State;
            school.VapiAssistantId = request.VapiAssistantId;
            school.ElevenLabsVoiceId = request.ElevenLabsVoiceId;
            school.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Schools.Update(school);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "SETTINGS_UPDATED", "School", schoolId.ToString());

            return await GetSettingsAsync(schoolId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings for school {SchoolId}", schoolId);
            return ApiResponse<SchoolSettingsDto>.Fail("Failed to update settings", "UPDATE_ERROR");
        }
    }

    public async Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse.Fail("User not found", "NOT_FOUND");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return ApiResponse.Fail("Current password is incorrect", "INVALID_PASSWORD");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(user.SchoolId, userId, "PASSWORD_CHANGED", "User", userId.ToString());

            return ApiResponse.Ok("Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return ApiResponse.Fail("Failed to change password", "CHANGE_PASSWORD_ERROR");
        }
    }
}
