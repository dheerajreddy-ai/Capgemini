using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Auth;

namespace EduVoice.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> RegisterSchoolAsync(RegisterSchoolRequest request);
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse> LogoutAsync(Guid userId);
    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request);
}
