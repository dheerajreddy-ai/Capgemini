using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Campaigns;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class CampaignService : ICampaignService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVapiService _vapiService;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(IUnitOfWork unitOfWork, IVapiService vapiService,
        IAuditService auditService, IConfiguration configuration, ILogger<CampaignService> logger)
    {
        _unitOfWork = unitOfWork;
        _vapiService = vapiService;
        _auditService = auditService;
        _configuration = configuration;
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

            return ApiResponse<PagedResult<CampaignDto>>.Ok(new PagedResult<CampaignDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = total, Page = page, PageSize = pageSize
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

            if (campaign == null) return ApiResponse<CampaignDetailDto>.Fail("Campaign not found", "NOT_FOUND");
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
            // Phase 1: reject schedules outside calling window
            if (request.ScheduledAt.HasValue && !CallingWindowHelper.IsWithinCallingWindow(request.ScheduledAt.Value))
                return ApiResponse<CampaignDto>.Fail(
                    $"Scheduled time must be within calling hours ({CallingWindowHelper.WindowDescription})", "OUTSIDE_WINDOW");

            // Phase 6: block if carrier is flagged
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school?.CarrierHealth == CarrierHealth.Flagged)
                return ApiResponse<CampaignDto>.Fail("Campaigns blocked — Twilio number is flagged by carrier. Contact support.", "CARRIER_FLAGGED");

            var studentsQuery = BuildStudentsQuery(schoolId, request.FilterClass, request.FilterSection,
                request.FilterFeesStatus, request.Type, school?.AttendanceAlertThreshold ?? 75);

            // Skip disputed and scholarship students for fee reminders
            if (request.Type == CampaignType.FeeReminder)
                studentsQuery = studentsQuery.Where(s => !s.HasFeeDispute && !s.IsScholarship);

            var studentCount = await studentsQuery.CountAsync();
            var status = request.ScheduledAt.HasValue ? CampaignStatus.Scheduled : CampaignStatus.Draft;

            var campaign = new CallCampaign
            {
                Id = Guid.NewGuid(), SchoolId = schoolId, Name = request.Name,
                Type = request.Type, Status = status, Description = request.Description,
                FilterClass = request.FilterClass, FilterSection = request.FilterSection,
                FilterFeesStatus = request.FilterFeesStatus, TotalStudents = studentCount,
                CustomMessage = request.CustomMessage, ScheduledAt = request.ScheduledAt,
                CreatedByUserId = createdByUserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Campaigns.AddAsync(campaign);
            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogAsync(schoolId, createdByUserId, "CAMPAIGN_CREATED", "Campaign", campaign.Id.ToString());

            if (!request.ScheduledAt.HasValue)
            {
                // Phase 1: enforce calling window for immediate campaigns
                if (!CallingWindowHelper.IsWithinCallingWindow())
                    return ApiResponse<CampaignDto>.Fail(
                        $"Cannot start campaign outside calling hours ({CallingWindowHelper.WindowDescription}). " +
                        $"Schedule it or wait until {CallingWindowHelper.NextWindowOpen():HH:mm} UTC.", "OUTSIDE_WINDOW");

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

    public async Task<ApiResponse> StartCampaignAsync(Guid schoolId, Guid campaignId)
    {
        try
        {
            if (!CallingWindowHelper.IsWithinCallingWindow())
                return ApiResponse.Fail($"Outside calling hours ({CallingWindowHelper.WindowDescription})", "OUTSIDE_WINDOW");

            var campaign = await _unitOfWork.Campaigns.Query()
                .Where(c => c.SchoolId == schoolId && c.Id == campaignId)
                .FirstOrDefaultAsync();

            if (campaign == null) return ApiResponse.Fail("Campaign not found", "NOT_FOUND");
            if (campaign.Status != CampaignStatus.Scheduled)
                return ApiResponse.Fail("Only Scheduled campaigns can be started this way", "INVALID_STATUS");

            _ = Task.Run(async () => await ProcessBulkCampaignAsync(campaignId, schoolId));
            return ApiResponse.Ok("Campaign started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting campaign {CampaignId}", campaignId);
            return ApiResponse.Fail("Failed to start campaign", "START_ERROR");
        }
    }

    public async Task<ApiResponse> SetDoNotCallAsync(Guid schoolId, Guid studentId)
    {
        try
        {
            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && s.Id == studentId)
                .FirstOrDefaultAsync();

            if (student == null) return ApiResponse.Fail("Student not found", "NOT_FOUND");

            student.DoNotCall = true;
            student.DoNotCallSetAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Do-not-call flag set. Parent will not be called in future campaigns.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting do-not-call for student {StudentId}", studentId);
            return ApiResponse.Fail("Failed to set do-not-call flag", "ERROR");
        }
    }

    public async Task<ApiResponse> PauseCampaignAsync(Guid schoolId, Guid campaignId)
    {
        try
        {
            var campaign = await _unitOfWork.Campaigns.Query()
                .Where(c => c.SchoolId == schoolId && c.Id == campaignId)
                .FirstOrDefaultAsync();

            if (campaign == null) return ApiResponse.Fail("Campaign not found", "NOT_FOUND");
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
            if (!CallingWindowHelper.IsWithinCallingWindow())
                return ApiResponse.Fail($"Cannot resume outside calling hours ({CallingWindowHelper.WindowDescription})", "OUTSIDE_WINDOW");

            var campaign = await _unitOfWork.Campaigns.Query()
                .Where(c => c.SchoolId == schoolId && c.Id == campaignId)
                .FirstOrDefaultAsync();

            if (campaign == null) return ApiResponse.Fail("Campaign not found", "NOT_FOUND");
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

            if (campaign == null) return ApiResponse.Fail("Campaign not found", "NOT_FOUND");
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

    public ApiResponse<CallingWindowStatusDto> GetCallingWindowStatus()
    {
        var isOpen = CallingWindowHelper.IsWithinCallingWindow();
        return ApiResponse<CallingWindowStatusDto>.Ok(new CallingWindowStatusDto
        {
            IsOpen = isOpen,
            Window = CallingWindowHelper.WindowDescription,
            NextOpenUtc = isOpen ? null : CallingWindowHelper.NextWindowOpen()
        });
    }

    private async Task ProcessBulkCampaignAsync(Guid campaignId, Guid schoolId)
    {
        try
        {
            var campaign = await _unitOfWork.Campaigns.Query().Where(c => c.Id == campaignId).FirstOrDefaultAsync();
            if (campaign == null) return;

            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null) return;

            // Phase 6: halve wave size if carrier is degraded
            var waveSize = school.CarrierHealth == CarrierHealth.Degraded ? campaign.WaveSize / 2 : campaign.WaveSize;
            waveSize = Math.Max(waveSize, 5);

            campaign.Status = CampaignStatus.Running;
            campaign.StartedAt = DateTime.UtcNow;
            campaign.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Campaigns.Update(campaign);
            await _unitOfWork.SaveChangesAsync();

            var studentsQuery = BuildStudentsQuery(schoolId, campaign.FilterClass, campaign.FilterSection,
                campaign.FilterFeesStatus, campaign.Type, school.AttendanceAlertThreshold);

            if (campaign.Type == CampaignType.FeeReminder)
                studentsQuery = studentsQuery.Where(s => !s.HasFeeDispute && !s.IsScholarship);

            var students = await studentsQuery.ToListAsync();

            // Phase 2: duplicate guard — skip students already reached today
            var today = DateTime.UtcNow.Date;
            var alreadyCalledToday = await _unitOfWork.Calls.Query()
                .Where(c => c.SchoolId == schoolId && c.CreatedAt >= today
                    && c.Status == CallStatus.Completed)
                .Select(c => c.StudentId)
                .ToHashSetAsync();

            students = students.Where(s => !alreadyCalledToday.Contains(s.Id)).ToList();

            // Phase 6: DND scrub
            if (school.DndScrubEnabled)
            {
                var dndApiKey = _configuration["DND_API_KEY"];
                var phones = students.Select(s => s.ParentPhone);
                var dndBlocked = await DndHelper.GetDndNumbersAsync(phones, dndApiKey, _logger);
                students = students.Where(s => !dndBlocked.Contains(s.ParentPhone)).ToList();
            }

            var waves = students.Chunk(waveSize).ToList();

            for (int waveIdx = campaign.CurrentWave; waveIdx < waves.Count; waveIdx++)
            {
                // Phase 1: auto-pause at end of calling window
                if (!CallingWindowHelper.IsWithinCallingWindow())
                {
                    var pausedCampaign = await _unitOfWork.Campaigns.Query().Where(c => c.Id == campaignId).FirstOrDefaultAsync();
                    if (pausedCampaign != null)
                    {
                        pausedCampaign.Status = CampaignStatus.Paused;
                        pausedCampaign.CurrentWave = waveIdx;
                        pausedCampaign.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Campaigns.Update(pausedCampaign);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    _logger.LogInformation("Campaign {CampaignId} auto-paused at wave {Wave} — outside calling window", campaignId, waveIdx);
                    return;
                }

                var wave = waves[waveIdx];
                foreach (var student in wave)
                {
                    var freshCampaign = await _unitOfWork.Campaigns.Query().Where(c => c.Id == campaignId).FirstOrDefaultAsync();
                    if (freshCampaign?.Status is CampaignStatus.Paused or CampaignStatus.Failed) return;

                    // Phase 1: skip do-not-call and fee extension students
                    if (student.DoNotCall || (student.HasFeeExtension && student.FeeExtensionUntil > DateTime.UtcNow))
                        continue;

                    var callType = campaign.Type switch
                    {
                        CampaignType.FeeReminder => CallType.FeeReminder,
                        CampaignType.ProgressUpdate => CallType.ProgressUpdate,
                        CampaignType.AttendanceAlert => CallType.AttendanceAlert,
                        _ => CallType.FeeReminder
                    };

                    try
                    {
                        var vapiCallId = await _vapiService.InitiateOutboundCallAsync(student, school, callType, campaignId);

                        var call = new Call
                        {
                            Id = Guid.NewGuid(), SchoolId = schoolId, StudentId = student.Id,
                            CampaignId = campaignId, VapiCallId = vapiCallId, Type = callType,
                            Direction = CallDirection.Outbound, Status = CallStatus.Initiated,
                            ToPhone = student.ParentPhone, FromPhone = school.TwilioPhoneNumber,
                            Language = school.DefaultCallLanguage,
                            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
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
                        var fc = await _unitOfWork.Campaigns.Query().Where(c => c.Id == campaignId).FirstOrDefaultAsync();
                        if (fc != null) { fc.CallsFailed++; fc.UpdatedAt = DateTime.UtcNow; _unitOfWork.Campaigns.Update(fc); await _unitOfWork.SaveChangesAsync(); }
                    }
                }

                // Phase 2: wave gap between waves (skip after last wave)
                if (waveIdx < waves.Count - 1)
                {
                    var gapMs = campaign.WaveGapMinutes * 60 * 1000;
                    _logger.LogInformation("Campaign {Id} wave {W} done, waiting {Gap}m before next wave", campaignId, waveIdx + 1, campaign.WaveGapMinutes);
                    await Task.Delay(gapMs);
                }
            }

            var finalCampaign = await _unitOfWork.Campaigns.Query().Where(c => c.Id == campaignId).FirstOrDefaultAsync();
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
                var campaign = await _unitOfWork.Campaigns.Query().Where(c => c.Id == campaignId).FirstOrDefaultAsync();
                if (campaign != null) { campaign.Status = CampaignStatus.Failed; campaign.UpdatedAt = DateTime.UtcNow; _unitOfWork.Campaigns.Update(campaign); await _unitOfWork.SaveChangesAsync(); }
            }
            catch { /* best effort */ }
        }
    }

    private IQueryable<Student> BuildStudentsQuery(Guid schoolId, string? filterClass,
        string? filterSection, FeesStatus? filterFees, CampaignType type, decimal attendanceThreshold)
    {
        var q = _unitOfWork.Students.Query().Where(s => s.SchoolId == schoolId && !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filterClass)) q = q.Where(s => s.Class == filterClass);
        if (!string.IsNullOrWhiteSpace(filterSection)) q = q.Where(s => s.Section == filterSection);
        if (filterFees.HasValue) q = q.Where(s => s.FeesStatus == filterFees.Value);

        // Phase 6: attendance alert only targets students below threshold
        if (type == CampaignType.AttendanceAlert)
            q = q.Where(s => s.AttendancePercentage.HasValue && s.AttendancePercentage < attendanceThreshold);

        return q;
    }

    private static CampaignDto MapToDto(CallCampaign c) => new()
    {
        Id = c.Id, Name = c.Name, Type = c.Type, Status = c.Status, Description = c.Description,
        TotalStudents = c.TotalStudents, CallsInitiated = c.CallsInitiated, CallsCompleted = c.CallsCompleted,
        CallsFailed = c.CallsFailed, CallsNoAnswer = c.CallsNoAnswer,
        ProgressPercent = c.TotalStudents > 0 ? Math.Round((double)(c.CallsCompleted + c.CallsFailed) / c.TotalStudents * 100, 1) : 0,
        ScheduledAt = c.ScheduledAt, StartedAt = c.StartedAt, CompletedAt = c.CompletedAt, CreatedAt = c.CreatedAt
    };

    private static CampaignDetailDto MapToDetailDto(CallCampaign c) => new()
    {
        Id = c.Id, Name = c.Name, Type = c.Type, Status = c.Status, Description = c.Description,
        FilterClass = c.FilterClass, FilterSection = c.FilterSection, FilterFeesStatus = c.FilterFeesStatus,
        TotalStudents = c.TotalStudents, CallsInitiated = c.CallsInitiated, CallsCompleted = c.CallsCompleted,
        CallsFailed = c.CallsFailed, CallsNoAnswer = c.CallsNoAnswer,
        ProgressPercent = c.TotalStudents > 0 ? Math.Round((double)(c.CallsCompleted + c.CallsFailed) / c.TotalStudents * 100, 1) : 0,
        CustomMessage = c.CustomMessage, ScheduledAt = c.ScheduledAt, StartedAt = c.StartedAt,
        CompletedAt = c.CompletedAt, CreatedByUserId = c.CreatedByUserId, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
    };
}

public class CallingWindowStatusDto
{
    public bool IsOpen { get; set; }
    public string Window { get; set; } = string.Empty;
    public DateTime? NextOpenUtc { get; set; }
}
