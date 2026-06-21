using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Broadcasts;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class BroadcastService : IBroadcastService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITwilioService _twilioService;
    private readonly ILogger<BroadcastService> _logger;

    public BroadcastService(IUnitOfWork unitOfWork, ITwilioService twilioService, ILogger<BroadcastService> logger)
    {
        _unitOfWork = unitOfWork;
        _twilioService = twilioService;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<BroadcastDto>>> GetBroadcastsAsync(Guid schoolId, int page, int pageSize)
    {
        try
        {
            var query = _unitOfWork.Broadcasts.Query()
                .Where(b => b.SchoolId == schoolId)
                .OrderByDescending(b => b.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return ApiResponse<PagedResult<BroadcastDto>>.Ok(new PagedResult<BroadcastDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = total, Page = page, PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching broadcasts for school {SchoolId}", schoolId);
            return ApiResponse<PagedResult<BroadcastDto>>.Fail("Failed to fetch broadcasts", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<BroadcastDto>> GetByIdAsync(Guid schoolId, Guid broadcastId)
    {
        try
        {
            var broadcast = await _unitOfWork.Broadcasts.Query()
                .Where(b => b.SchoolId == schoolId && b.Id == broadcastId)
                .FirstOrDefaultAsync();

            if (broadcast == null)
                return ApiResponse<BroadcastDto>.Fail("Broadcast not found", "NOT_FOUND");

            return ApiResponse<BroadcastDto>.Ok(MapToDto(broadcast));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching broadcast {BroadcastId}", broadcastId);
            return ApiResponse<BroadcastDto>.Fail("Failed to fetch broadcast", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<BroadcastDto>> CreateAndSendAsync(Guid schoolId, Guid userId, CreateBroadcastRequest request)
    {
        try
        {
            // Build target recipients
            var studentsQuery = _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && !s.IsDeleted && !s.DoNotCall);

            if (!string.IsNullOrWhiteSpace(request.TargetClass))
                studentsQuery = studentsQuery.Where(s => s.Class == request.TargetClass);
            if (!string.IsNullOrWhiteSpace(request.TargetSection))
                studentsQuery = studentsQuery.Where(s => s.Section == request.TargetSection);

            var recipients = await studentsQuery
                .Select(s => new { s.ParentPhone, s.ParentName, s.FirstName, s.LastName, s.Class })
                .Distinct()
                .ToListAsync();

            var broadcast = new Broadcast
            {
                Id = Guid.NewGuid(), SchoolId = schoolId,
                Title = request.Title, Message = request.Message,
                MediaUrl = request.MediaUrl, MediaType = request.MediaType,
                TargetClass = request.TargetClass, TargetSection = request.TargetSection,
                Status = request.SendNow ? BroadcastStatus.Sending : BroadcastStatus.Draft,
                TotalRecipients = recipients.Count,
                CreatedByUserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Broadcasts.AddAsync(broadcast);
            await _unitOfWork.SaveChangesAsync();

            if (request.SendNow && recipients.Count > 0)
            {
                // Fire-and-forget — send in background
                _ = Task.Run(async () => await SendBulkAsync(broadcast.Id, schoolId, recipients
                    .Select(r => r.ParentPhone).ToList(), request.Message, request.MediaUrl, request.MediaType));
            }

            return ApiResponse<BroadcastDto>.Ok(MapToDto(broadcast),
                $"Broadcast created. Sending to {recipients.Count} parents.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating broadcast for school {SchoolId}", schoolId);
            return ApiResponse<BroadcastDto>.Fail("Failed to create broadcast", "CREATE_ERROR");
        }
    }

    private async Task SendBulkAsync(Guid broadcastId, Guid schoolId, List<string> phones,
        string message, string? mediaUrl, BroadcastMediaType mediaType)
    {
        var sent = 0; var failed = 0;

        foreach (var phone in phones)
        {
            try
            {
                if (!string.IsNullOrEmpty(mediaUrl) && mediaType != BroadcastMediaType.None)
                    await _twilioService.SendWhatsAppWithMediaAsync(phone, message, mediaUrl);
                else
                    await _twilioService.SendWhatsAppAsync(phone, message);

                sent++;
                // Throttle — Twilio rate limit is ~1 msg/s per account on WhatsApp
                await Task.Delay(1100);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send broadcast to {Phone}", phone);
                failed++;
            }
        }

        // Update final status
        try
        {
            var broadcast = await _unitOfWork.Broadcasts.Query()
                .Where(b => b.Id == broadcastId).FirstOrDefaultAsync();
            if (broadcast != null)
            {
                broadcast.SentCount = sent;
                broadcast.FailedCount = failed;
                broadcast.Status = failed == phones.Count ? BroadcastStatus.Failed : BroadcastStatus.Sent;
                broadcast.SentAt = DateTime.UtcNow;
                broadcast.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Broadcasts.Update(broadcast);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating broadcast status for {BroadcastId}", broadcastId);
        }

        _logger.LogInformation("Broadcast {Id}: sent={Sent}, failed={Failed}", broadcastId, sent, failed);
    }

    private static BroadcastDto MapToDto(Broadcast b) => new()
    {
        Id = b.Id, Title = b.Title, Message = b.Message,
        MediaUrl = b.MediaUrl, MediaType = b.MediaType,
        TargetClass = b.TargetClass, TargetSection = b.TargetSection,
        Status = b.Status, TotalRecipients = b.TotalRecipients,
        SentCount = b.SentCount, FailedCount = b.FailedCount,
        SentAt = b.SentAt, CreatedAt = b.CreatedAt
    };
}
