using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Campaigns;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class CampaignService : ICampaignService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVapiService _vapiService;
    private readonly IAuditService _auditService;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(IUnitOfWork unitOfWork, IVapiService vapiService,
        IAuditService auditService, ILogger<CampaignService> logger)
    {
        _unitOfWork = unitOfWork;
        _vapiService = vapiService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<CampaignDto>>> GetCampaignsAsync(Guid schoolId, int page, int pageSize)
    {
        try
        {
            var query = _unitOfWork.Campaigns.Query()
                .Where(c => c.SchoolId == schoolId)
                .OrderByDescending(c => c.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var dtos = items.Select(MapToDto).ToList();

            return ApiResponse<PagedResult<CampaignDto>>.Ok(new PagedResult<CampaignDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaigns for school {SchoolId}", schoolId);
            return ApiResponse<PagedResult<CampaignDto>>.Fail("Failed to fetch campaigns", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<CampaignDetailDto>> GetCampaignByIdAsync(Guid schoolId, Guid campaignId)
    {
        try
        {
            var campaign = await _unitOfWork.Campaigns.Query()
                .Where(c => c.SchoolId == schoolId && c.Id == campaignId)
                .FirstOrDefaultAsync();

            if (campaign == null)
                return ApiResponse<CampaignDetailDto>.Fail("Campaign not found", "NOT_FOUND");

            return ApiResponse<CampaignDetailDto>.Ok(MapToDetailDto(campaign));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching campaign {CampaignId}", campaignId);
            return ApiResponse<CampaignDetailDto>.Fail("Failed to fetch campaign", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<CampaignDto>> CreateCampaignAsync(Guid schoolId, Guid createdByUserId, CreateCampaignRequest request)
    {
        try
        {
            var studentsQuery = _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.FilterClass))
                studentsQuery = studentsQuery.Where(s => s.Class == request.FilterClass);

            if (!string.IsNullOrWhiteSpace(request.FilterSection))
                studentsQuery = studentsQuery.Where(s => s.Section == request.FilterSection);

            if (request.FilterFeesStatus.HasValue)
                studentsQuery = studentsQuery.Where(s => s.FeesStatus == request.FilterFeesStatus.Value);

            var studentCount = await studentsQuery.CountAsync();

            var campaign = new CallCampaign
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                Name = request.Name,
                Type = request.Type,
                Status = CampaignStatus.Draft,
                Description = request.Description,
                FilterClass = request.FilterClass,
                FilterSection = request.FilterSection,
                FilterFeesStatus = request.FilterFeesStatus,
                TotalStudents = studentCount,
                CustomMessage = request.CustomMessage,
                ScheduledAt = request.ScheduledAt,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Campaigns.AddAsync(campaign);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, createdByUserId, "CAMPAIGN_CREATED", "Campaign", campaign.Id.ToString());

            if (!request.ScheduledAt.HasValue)
            {
                _ = Task.Run(async () => await ProcessBulkCampaignAsync(campaign.Id, schoolId));
            }

            return ApiResponse<CampaignDto>.Ok(MapToDto(campaign), "Campaign created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign for school {SchoolId}", schoolId);
            return ApiResponse<CampaignDto>.Fail("Failed to create campaign", "CREATE_ERROR");
        }
    }

    public async Task<ApiResponse> PauseCampaignAsync(Guid schoolId, Guid campaignId)
    {
        try
        {
            var campaign = await _unitOfWork.Campaigns.Query()
                .Where(c => c.SchoolId == schoolId && c.Id == campaignId)
                .FirstOrDefaultAsync();

            if (campaign == null)
                return ApiResponse.Fail("Campaign not found", "NOT_FOUND");

            if (campaign.Status != CampaignStatus.Running)
                return ApiResponse.Fail("Campaign is not running", "INVALID_STATUS");

            campaign.Status = CampaignStatus.Paused;
            campaign.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Campaigns.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Campaign paused");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing campaign {CampaignId}", campaignId);
            return ApiResponse.Fail("Failed to pause campaign", "PAUSE_ERROR");
        }
    }

    public async Task<ApiResponse> ResumeCampaignAsync(Guid schoolId, Guid campaignId)
    {
        try
        {
            var campaign = await _unitOfWork.Campaigns.Query()
                .Where(c => c.SchoolId == schoolId && c.Id == campaignId)
                .FirstOrDefaultAsync();

            if (campaign == null)
                return ApiResponse.Fail("Campaign not found", "NOT_FOUND");

            if (campaign.Status != CampaignStatus.Paused)
                return ApiResponse.Fail("Campaign is not paused", "INVALID_STATUS");

            campaign.Status = CampaignStatus.Running;
            campaign.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Campaigns.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            _ = Task.Run(async () => await ProcessBulkCampaignAsync(campaign.Id, schoolId));

            return ApiResponse.Ok("Campaign resumed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming campaign {CampaignId}", campaignId);
            return ApiResponse.Fail("Failed to resume campaign", "RESUME_ERROR");
        }
    }

    public async Task<ApiResponse> CancelCampaignAsync(Guid schoolId, Guid campaignId)
    {
        try
        {
            var campaign = await _unitOfWork.Campaigns.Query()
                .Where(c => c.SchoolId == schoolId && c.Id == campaignId)
                .FirstOrDefaultAsync();

            if (campaign == null)
                return ApiResponse.Fail("Campaign not found", "NOT_FOUND");

            if (campaign.Status == CampaignStatus.Completed)
                return ApiResponse.Fail("Campaign already completed", "INVALID_STATUS");

            campaign.Status = CampaignStatus.Failed;
            campaign.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Campaigns.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Campaign cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling campaign {CampaignId}", campaignId);
            return ApiResponse.Fail("Failed to cancel campaign", "CANCEL_ERROR");
        }
    }

    private async Task ProcessBulkCampaignAsync(Guid campaignId, Guid schoolId)
    {
        try
        {
            var campaign = await _unitOfWork.Campaigns.Query()
                .Where(c => c.Id == campaignId)
                .FirstOrDefaultAsync();

            if (campaign == null) return;

            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null) return;

            campaign.Status = CampaignStatus.Running;
            campaign.StartedAt = DateTime.UtcNow;
            campaign.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Campaigns.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            var studentsQuery = _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(campaign.FilterClass))
                studentsQuery = studentsQuery.Where(s => s.Class == campaign.FilterClass);

            if (!string.IsNullOrWhiteSpace(campaign.FilterSection))
                studentsQuery = studentsQuery.Where(s => s.Section == campaign.FilterSection);

            if (campaign.FilterFeesStatus.HasValue)
                studentsQuery = studentsQuery.Where(s => s.FeesStatus == campaign.FilterFeesStatus.Value);

            var students = await studentsQuery.ToListAsync();

            foreach (var student in students)
            {
                var freshCampaign = await _unitOfWork.Campaigns.Query()
                    .Where(c => c.Id == campaignId)
                    .FirstOrDefaultAsync();

                if (freshCampaign?.Status == CampaignStatus.Paused ||
                    freshCampaign?.Status == CampaignStatus.Failed)
                    break;

                try
                {
                    var vapiCallId = await _vapiService.InitiateOutboundCallAsync(student, school, campaign.Type switch
                    {
                        CampaignType.FeeReminder => CallType.FeeReminder,
                        CampaignType.ProgressUpdate => CallType.ProgressUpdate,
                        _ => CallType.FeeReminder
                    }, campaign.Id);

                    var call = new Call
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = schoolId,
                        StudentId = student.Id,
                        CampaignId = campaignId,
                        VapiCallId = vapiCallId,
                        Type = campaign.Type switch
                        {
                            CampaignType.FeeReminder => CallType.FeeReminder,
                            CampaignType.ProgressUpdate => CallType.ProgressUpdate,
                            _ => CallType.FeeReminder
                        },
                        Direction = CallDirection.Outbound,
                        Status = CallStatus.Initiated,
                        ToPhone = student.ParentPhone,
                        FromPhone = school.TwilioPhoneNumber,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Calls.AddAsync(call);

                    if (freshCampaign != null)
                    {
                        freshCampaign.CallsInitiated++;
                        freshCampaign.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Campaigns.Update(freshCampaign);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await Task.Delay(2000);
                }
                catch (Exception callEx)
                {
                    _logger.LogWarning(callEx, "Failed to initiate call for student {StudentId}", student.Id);

                    var freshCampaign2 = await _unitOfWork.Campaigns.Query()
                        .Where(c => c.Id == campaignId)
                        .FirstOrDefaultAsync();

                    if (freshCampaign2 != null)
                    {
                        freshCampaign2.CallsFailed++;
                        freshCampaign2.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Campaigns.Update(freshCampaign2);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }

            var finalCampaign = await _unitOfWork.Campaigns.Query()
                .Where(c => c.Id == campaignId)
                .FirstOrDefaultAsync();

            if (finalCampaign != null && finalCampaign.Status == CampaignStatus.Running)
            {
                finalCampaign.Status = CampaignStatus.Completed;
                finalCampaign.CompletedAt = DateTime.UtcNow;
                finalCampaign.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Campaigns.Update(finalCampaign);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bulk campaign {CampaignId}", campaignId);

            try
            {
                var campaign = await _unitOfWork.Campaigns.Query()
                    .Where(c => c.Id == campaignId)
                    .FirstOrDefaultAsync();

                if (campaign != null)
                {
                    campaign.Status = CampaignStatus.Failed;
                    campaign.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Campaigns.Update(campaign);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch { /* best effort */ }
        }
    }

    private static CampaignDto MapToDto(CallCampaign c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Type = c.Type,
        Status = c.Status,
        Description = c.Description,
        TotalStudents = c.TotalStudents,
        CallsInitiated = c.CallsInitiated,
        CallsCompleted = c.CallsCompleted,
        CallsFailed = c.CallsFailed,
        CallsNoAnswer = c.CallsNoAnswer,
        ProgressPercent = c.TotalStudents > 0
            ? Math.Round((double)(c.CallsCompleted + c.CallsFailed) / c.TotalStudents * 100, 1)
            : 0,
        ScheduledAt = c.ScheduledAt,
        StartedAt = c.StartedAt,
        CompletedAt = c.CompletedAt,
        CreatedAt = c.CreatedAt
    };

    private static CampaignDetailDto MapToDetailDto(CallCampaign c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Type = c.Type,
        Status = c.Status,
        Description = c.Description,
        FilterClass = c.FilterClass,
        FilterSection = c.FilterSection,
        FilterFeesStatus = c.FilterFeesStatus,
        TotalStudents = c.TotalStudents,
        CallsInitiated = c.CallsInitiated,
        CallsCompleted = c.CallsCompleted,
        CallsFailed = c.CallsFailed,
        CallsNoAnswer = c.CallsNoAnswer,
        ProgressPercent = c.TotalStudents > 0
            ? Math.Round((double)(c.CallsCompleted + c.CallsFailed) / c.TotalStudents * 100, 1)
            : 0,
        CustomMessage = c.CustomMessage,
        ScheduledAt = c.ScheduledAt,
        StartedAt = c.StartedAt,
        CompletedAt = c.CompletedAt,
        CreatedByUserId = c.CreatedByUserId,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
