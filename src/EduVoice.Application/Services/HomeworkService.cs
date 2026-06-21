using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Homework;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class HomeworkService : IHomeworkService
{
    private readonly IUnitOfWork _uow;
    private readonly ITwilioService _twilioService;
    private readonly ILogger<HomeworkService> _logger;

    public HomeworkService(IUnitOfWork uow, ITwilioService twilioService, ILogger<HomeworkService> logger)
    {
        _uow = uow;
        _twilioService = twilioService;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<HomeworkDto>>> GetAllAsync(Guid schoolId, int page, int pageSize, DateTime? date)
    {
        try
        {
            var query = _uow.Homeworks.Query().Where(h => h.SchoolId == schoolId);
            if (date.HasValue)
                query = query.Where(h => h.AssignedDate.Date == date.Value.Date);
            query = query.OrderByDescending(h => h.AssignedDate);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return ApiResponse<PagedResult<HomeworkDto>>.Ok(new PagedResult<HomeworkDto>
            {
                Items = items.Select(Map).ToList(), TotalCount = total, Page = page, PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching homework for school {SchoolId}", schoolId);
            return ApiResponse<PagedResult<HomeworkDto>>.Fail("Failed to fetch homework", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<HomeworkDto>> GetByIdAsync(Guid schoolId, Guid id)
    {
        var item = await _uow.Homeworks.Query()
            .FirstOrDefaultAsync(h => h.SchoolId == schoolId && h.Id == id);
        return item is null
            ? ApiResponse<HomeworkDto>.Fail("Not found", "NOT_FOUND")
            : ApiResponse<HomeworkDto>.Ok(Map(item));
    }

    public async Task<ApiResponse<HomeworkDto>> CreateAsync(Guid schoolId, Guid userId, CreateHomeworkRequest request)
    {
        try
        {
            var entity = new Homework
            {
                Id = Guid.NewGuid(), SchoolId = schoolId,
                Subject = request.Subject, Description = request.Description,
                Class = request.Class, Section = request.Section,
                AssignedDate = request.AssignedDate.ToUniversalTime(),
                DueDate = request.DueDate?.ToUniversalTime(),
                CreatedByUserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            await _uow.Homeworks.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return ApiResponse<HomeworkDto>.Ok(Map(entity), "Homework added.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating homework");
            return ApiResponse<HomeworkDto>.Fail("Failed to create homework", "CREATE_ERROR");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid schoolId, Guid id)
    {
        var item = await _uow.Homeworks.Query()
            .FirstOrDefaultAsync(h => h.SchoolId == schoolId && h.Id == id);
        if (item is null) return ApiResponse<bool>.Fail("Not found", "NOT_FOUND");
        _uow.Homeworks.Remove(item);
        await _uow.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    public async Task SendDailyAlertsAsync()
    {
        var today = DateTime.UtcNow.Date;

        // All homework assigned today that hasn't been alerted yet
        var homeworkList = await _uow.Homeworks.Query()
            .Where(h => h.AssignedDate.Date == today && !h.AlertSent)
            .ToListAsync();

        if (homeworkList.Count == 0) return;

        // Group by school
        var bySchool = homeworkList.GroupBy(h => h.SchoolId);

        foreach (var schoolGroup in bySchool)
        {
            var school = await _uow.Schools.GetByIdAsync(schoolGroup.Key);
            if (school is null) continue;

            // Group by class+section within the school
            var byClass = schoolGroup.GroupBy(h => new { h.Class, h.Section });

            foreach (var classGroup in byClass)
            {
                var studentsQuery = _uow.Students.Query()
                    .Where(s => s.SchoolId == schoolGroup.Key && !s.IsDeleted && !s.DoNotCall);

                if (!string.IsNullOrWhiteSpace(classGroup.Key.Class))
                    studentsQuery = studentsQuery.Where(s => s.Class == classGroup.Key.Class);
                if (!string.IsNullOrWhiteSpace(classGroup.Key.Section))
                    studentsQuery = studentsQuery.Where(s => s.Section == classGroup.Key.Section);

                var students = await studentsQuery.ToListAsync();

                // Build the message listing all subjects
                var hwLines = classGroup.Select(h =>
                    $"ðŸ“š *{h.Subject}*: {h.Description}" +
                    (h.DueDate.HasValue ? $" (Due: {h.DueDate:dd MMM})" : ""));
                var message = $"""
                    ðŸ« *{school.Name}* â€” Today's Homework ({today:dd MMM yyyy})
                    {(classGroup.Key.Class is not null ? $"Class: {classGroup.Key.Class}{classGroup.Key.Section}" : "")}

                    {string.Join("\n", hwLines)}

                    âœ… Please ensure your child completes all assignments.
                    """;

                foreach (var student in students)
                {
                    try
                    {
                        var phone = student.ParentPhone;
                        if (!string.IsNullOrWhiteSpace(phone))
                            await _twilioService.SendWhatsAppAsync(phone, message);
                        await Task.Delay(1100);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Homework alert failed for student {Id}", student.Id);
                    }
                }
            }

            // Mark alerts sent
            foreach (var hw in schoolGroup)
            {
                hw.AlertSent = true;
                hw.AlertSentAt = DateTime.UtcNow;
                hw.UpdatedAt = DateTime.UtcNow;
                _uow.Homeworks.Update(hw);
            }
            await _uow.SaveChangesAsync();
        }

        _logger.LogInformation("HomeworkService: daily alerts sent for {Count} homework entries", homeworkList.Count);
    }

    private static HomeworkDto Map(Homework h) => new()
    {
        Id = h.Id, Subject = h.Subject, Description = h.Description,
        Class = h.Class, Section = h.Section,
        AssignedDate = h.AssignedDate, DueDate = h.DueDate,
        AlertSent = h.AlertSent, AlertSentAt = h.AlertSentAt, CreatedAt = h.CreatedAt
    };
}
