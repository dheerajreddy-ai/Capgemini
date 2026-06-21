using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Portal;

namespace EduVoice.Application.Interfaces;

public interface IInboundService
{
    Task<ApiResponse> SendPortalOtpAsync(string phone, string subDomain);
    Task<ApiResponse<PortalDataDto>> VerifyPortalOtpAsync(string phone, string otp, string subDomain);
}
