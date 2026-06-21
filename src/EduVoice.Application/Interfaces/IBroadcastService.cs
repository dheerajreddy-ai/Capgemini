using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Broadcasts;

namespace EduVoice.Application.Interfaces;

public interface IBroadcastService
{
    Task<ApiResponse<PagedResult<BroadcastDto>>> GetBroadcastsAsync(Guid schoolId, int page, int pageSize);
    Task<ApiResponse<BroadcastDto>> CreateAndSendAsync(Guid schoolId, Guid userId, CreateBroadcastRequest request);
    Task<ApiResponse<BroadcastDto>> GetByIdAsync(Guid schoolId, Guid broadcastId);
}
