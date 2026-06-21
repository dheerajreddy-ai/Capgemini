using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Calls;
using EduVoice.Domain.Enums;

namespace EduVoice.Application.Interfaces;

public interface ICallService
{
    Task<ApiResponse<PagedResult<CallListDto>>> GetCallsAsync(Guid schoolId, int page, int pageSize,
        CallStatus? status, CallType? type, Guid? campaignId, DateTime? from, DateTime? to);
    Task<ApiResponse<CallDetailDto>> GetCallByIdAsync(Guid schoolId, Guid callId);
    Task ProcessVapiWebhookAsync(VapiWebhookPayload payload);
    Task<ApiResponse<CallDto>> RetryCallAsync(Guid schoolId, Guid callId);
    Task ProcessScheduledRetriesAsync();
}

public class VapiWebhookPayload
{
    public string Type { get; set; } = string.Empty;
    public VapiCallData? Call { get; set; }
    public string? Transcript { get; set; }
    public string? RecordingUrl { get; set; }
    public List<VapiTranscriptMessage>? Messages { get; set; }
    public VapiAnalysis? Analysis { get; set; }
    public string? EndedReason { get; set; }
}

public class VapiCallData
{
    public string Id { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Status { get; set; }
    public string? EndedReason { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class VapiTranscriptMessage
{
    public string Role { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double? Time { get; set; }
}

public class VapiAnalysis
{
    public string? Summary { get; set; }
    public string? SuccessEvaluation { get; set; }
}
