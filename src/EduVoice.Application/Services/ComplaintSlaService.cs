using EduVoice.Application.Common;
using EduVoice.Application.DTOs.ComplaintSla;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class ComplaintSlaService : IComplaintSlaService
{
    // Default SLA hours per category when no school-specific config exists
    private static readonly Dictionary<ComplaintCategory, int> DefaultSlaHours = new()
    {
        { ComplaintCategory.Teacher, 48 },
        { ComplaintCategory.Fees, 72 },
        { ComplaintCategory.Facility, 48 },
        { ComplaintCategory.Academic, 72 },
        { ComplaintCategory.Behaviour, 48 },
        { ComplaintCategory.Other, 96 },
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ComplaintSlaService> _logger;

    public ComplaintSlaService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IAuditService auditService,
        ILogger<ComplaintSlaService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<ComplaintSlaConfigDto>>> GetConfigsAsync(Guid schoolId)
    {
        try
        {
            var saved = await _unitOfWork.ComplaintSlaConfigs.Query()
                .Include(c => c.EscalationContact)
                .Where(c => c.SchoolId == schoolId)
                .ToListAsync();

            var result = Enum.GetValues<ComplaintCategory>().Select(cat =>
            {
                var config = saved.FirstOrDefault(c => c.Category == cat);
                return new ComplaintSlaConfigDto
                {
                    Id = config?.Id,
                    Category = cat,
                    SlaHours = config?.SlaHours ?? DefaultSlaHours[cat],
                    EscalationContactUserId = config?.EscalationContactUserId,
                    EscalationContactName = config?.EscalationContact != null
                        ? $"{config.EscalationContact.FirstName} {config.EscalationContact.LastName}".Trim()
                        : null,
                    EscalationContactEmail = config?.EscalationContact?.Email,
                    IsCustomised = config != null
                };
            }).ToList();

            return ApiResponse<List<ComplaintSlaConfigDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching SLA configs for school {SchoolId}", schoolId);
            return ApiResponse<List<ComplaintSlaConfigDto>>.Fail("Failed to fetch SLA configs", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<ComplaintSlaConfigDto>> UpsertConfigAsync(Guid schoolId, UpsertSlaConfigRequest request)
    {
        try
        {
            if (request.SlaHours < 1 || request.SlaHours > 8760)
                return ApiResponse<ComplaintSlaConfigDto>.Fail("SLA hours must be between 1 and 8760", "INVALID_SLA_HOURS");

            var existing = await _unitOfWork.ComplaintSlaConfigs.Query()
                .Where(c => c.SchoolId == schoolId && c.Category == request.Category)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.SlaHours = request.SlaHours;
                existing.EscalationContactUserId = request.EscalationContactUserId;
                _unitOfWork.ComplaintSlaConfigs.Update(existing);
            }
            else
            {
                existing = new ComplaintSlaConfig
                {
                    Id = Guid.NewGuid(),
                    SchoolId = schoolId,
                    Category = request.Category,
                    SlaHours = request.SlaHours,
                    EscalationContactUserId = request.EscalationContactUserId
                };
                await _unitOfWork.ComplaintSlaConfigs.AddAsync(existing);
            }

            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogAsync(schoolId, null, "SLA_CONFIG_UPDATED", "ComplaintSlaConfig", request.Category.ToString());

            var configs = await GetConfigsAsync(schoolId);
            var updated = configs.Data!.First(c => c.Category == request.Category);
            return ApiResponse<ComplaintSlaConfigDto>.Ok(updated, "SLA config saved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting SLA config for school {SchoolId}", schoolId);
            return ApiResponse<ComplaintSlaConfigDto>.Fail("Failed to save SLA config", "UPSERT_ERROR");
        }
    }

    public async Task CheckAndEscalateAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var schools = await _unitOfWork.Schools.Query().Where(s => s.IsActive).ToListAsync();

            foreach (var school in schools)
            {
                await BackfillSlaDeadlinesForSchoolAsync(school.Id, now);
                await EscalateBreachedComplaintsAsync(school.Id, now);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ComplaintSlaService.CheckAndEscalateAsync failed");
        }
    }

    private async Task BackfillSlaDeadlinesForSchoolAsync(Guid schoolId, DateTime now)
    {
        // Find open complaints with no SLA deadline yet
        var complaints = await _unitOfWork.Complaints.Query()
            .Where(c => c.SchoolId == schoolId
                && c.SlaDeadline == null
                && c.Status != ComplaintStatus.Resolved
                && c.Status != ComplaintStatus.Closed)
            .ToListAsync();

        if (complaints.Count == 0) return;

        var configs = await _unitOfWork.ComplaintSlaConfigs.Query()
            .Where(c => c.SchoolId == schoolId)
            .ToListAsync();

        foreach (var complaint in complaints)
        {
            var hours = configs.FirstOrDefault(c => c.Category == complaint.Category)?.SlaHours
                ?? DefaultSlaHours[complaint.Category];
            complaint.SlaDeadline = complaint.CreatedAt.AddHours(hours);
            _unitOfWork.Complaints.Update(complaint);
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Backfilled SLA deadlines for {Count} complaints in school {SchoolId}", complaints.Count, schoolId);
    }

    private async Task EscalateBreachedComplaintsAsync(Guid schoolId, DateTime now)
    {
        var breached = await _unitOfWork.Complaints.Query()
            .Where(c => c.SchoolId == schoolId
                && c.SlaDeadline != null
                && c.SlaDeadline < now
                && c.EscalatedAt == null
                && c.Status != ComplaintStatus.Resolved
                && c.Status != ComplaintStatus.Closed)
            .ToListAsync();

        if (breached.Count == 0) return;

        var configs = await _unitOfWork.ComplaintSlaConfigs.Query()
            .Include(c => c.EscalationContact)
            .Where(c => c.SchoolId == schoolId)
            .ToListAsync();

        foreach (var complaint in breached)
        {
            complaint.EscalatedAt = now;
            complaint.EscalationLevel = 1;
            _unitOfWork.Complaints.Update(complaint);

            var config = configs.FirstOrDefault(c => c.Category == complaint.Category);
            if (config?.EscalationContact != null)
            {
                try
                {
                    await _emailService.SendSlaEscalationEmailAsync(config.EscalationContact.Email, complaint);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send SLA escalation email for complaint {Id}", complaint.Id);
                }
            }

            await _auditService.LogAsync(schoolId, null, "COMPLAINT_SLA_BREACH", "Complaint", complaint.Id.ToString());
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Escalated {Count} SLA-breached complaints in school {SchoolId}", breached.Count, schoolId);
    }
}
