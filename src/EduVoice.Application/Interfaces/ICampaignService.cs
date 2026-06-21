using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Campaigns;

namespace EduVoice.Application.Interfaces;

public interface ICampaignService
{
    Task<ApiResponse<PagedResult<CampaignDto>>> GetCampaignsAsync(Guid schoolId, int page, int pageSize);
    Task<ApiResponse<CampaignDetailDto>> GetCampaignByIdAsync(Guid schoolId, Guid campaignId);
    Task<ApiResponse<CampaignDto>> CreateCampaignAsync(Guid schoolId, Guid createdByUserId, CreateCampaignRequest request);
    Task<ApiResponse> PauseCampaignAsync(Guid schoolId, Guid campaignId);
    Task<ApiResponse> ResumeCampaignAsync(Guid schoolId, Guid campaignId);
    Task<ApiResponse> CancelCampaignAsync(Guid schoolId, Guid campaignId);
}
