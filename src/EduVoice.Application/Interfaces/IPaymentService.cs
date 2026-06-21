using EduVoice.Application.Common;

namespace EduVoice.Application.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse> SendPaymentLinkAsync(Guid schoolId, Guid studentId);
    Task<ApiResponse> ConfirmPaymentAsync(Guid schoolId, Guid studentId, decimal amountPaid);
    Task<ApiResponse> RaiseDisputeAsync(Guid schoolId, Guid studentId, string note);
    Task<ApiResponse> ResolveDisputeAsync(Guid schoolId, Guid studentId);
    Task<ApiResponse> GrantExtensionAsync(Guid schoolId, Guid studentId, int extraDays);
}
