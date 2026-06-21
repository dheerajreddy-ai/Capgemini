using EduVoice.Application.Common;
using EduVoice.Application.DTOs.ExamSchedules;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class ExamScheduleService : IExamScheduleService
{
    private readonly IUnitOfWork _uow;
    private readonly IVapiService _vapiService;
    private readonly ILogger<ExamScheduleService> _logger;

    public ExamScheduleService(IUnitOfWork uow, IVapiService vapiService, ILogger<ExamScheduleService> logger)
    {
        _uow = uow;
        _vapiService = vapiService;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<ExamScheduleDto>>> GetAllAsync(Guid schoolId, int page, int pageSize)
    {
        try
        {
            var query = _uow.ExamSchedules.Query()
                .Where(e => e.SchoolId == schoolId && e.IsActive)
                .OrderBy(e => e.ExamDate);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return ApiResponse<PagedResult<ExamScheduleDto>>.Ok(new PagedResult<ExamScheduleDto>
            {
                Items = items.Select(Map).ToList(), TotalCount = total, Page = page, PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching exam schedules for school {SchoolId}", schoolId);
            return ApiResponse<PagedResult<ExamScheduleDto>>.Fail("Failed to fetch exam schedules", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<ExamScheduleDto>> GetByIdAsync(Guid schoolId, Guid id)
    {
        var item = await _uow.ExamSchedules.Query()
            .FirstOrDefaultAsync(e => e.SchoolId == schoolId && e.Id == id);
        return item is null
            ? ApiResponse<ExamScheduleDto>.Fail("Not found", "NOT_FOUND")
            : ApiResponse<ExamScheduleDto>.Ok(Map(item));
    }

    public async Task<ApiResponse<ExamScheduleDto>> CreateAsync(Guid schoolId, Guid userId, CreateExamScheduleRequest request)
    {
        try
        {
            var entity = new ExamSchedule
            {
                Id = Guid.NewGuid(), SchoolId = schoolId,
                SubjectName = request.SubjectName, ExamType = request.ExamType,
                ExamDate = request.ExamDate.ToUniversalTime(),
                Class = request.Class, Section = request.Section, Notes = request.Notes,
                CreatedByUserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            await _uow.ExamSchedules.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return ApiResponse<ExamScheduleDto>.Ok(Map(entity), "Exam schedule created.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exam schedule");
            return ApiResponse<ExamScheduleDto>.Fail("Failed to create exam schedule", "CREATE_ERROR");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid schoolId, Guid id)
    {
        var item = await _uow.ExamSchedules.Query()
            .FirstOrDefaultAsync(e => e.SchoolId == schoolId && e.Id == id);
        if (item is null) return ApiResponse<bool>.Fail("Not found", "NOT_FOUND");
        item.IsActive = false;
        item.UpdatedAt = DateTime.UtcNow;
        _uow.ExamSchedules.Update(item);
        await _uow.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true);
    }

    public async Task ProcessRemindersAsync()
    {
        var today = DateTime.UtcNow.Date;
        var in3Days = today.AddDays(3);
        var in1Day = today.AddDays(1);

        // Load all active exams due in 1 or 3 days with pending reminders
        var exams = await _uow.ExamSchedules.Query()
            .Where(e => e.IsActive && (
                (e.ExamDate.Date == in3Days && !e.Reminder3DaySent) ||
                (e.ExamDate.Date == in1Day && !e.Reminder1DaySent)))
            .ToListAsync();

        foreach (var exam in exams)
        {
            var is3Day = exam.ExamDate.Date == in3Days && !exam.Reminder3DaySent;
            var is1Day = exam.ExamDate.Date == in1Day && !exam.Reminder1DaySent;

            // Get matching students (by class/section if specified)
            var studentsQuery = _uow.Students.Query()
                .Where(s => s.SchoolId == exam.SchoolId && !s.IsDeleted && !s.DoNotCall);

            if (!string.IsNullOrWhiteSpace(exam.Class))
                studentsQuery = studentsQuery.Where(s => s.Class == exam.Class);
            if (!string.IsNullOrWhiteSpace(exam.Section))
                studentsQuery = studentsQuery.Where(s => s.Section == exam.Section);

            var students = await studentsQuery.ToListAsync();
            var school = await _uow.Schools.GetByIdAsync(exam.SchoolId);
            if (school is null) continue;

            var daysLabel = is1Day ? "à°°à±‡à°ªà± (Tomorrow)" : "3 à°°à±‹à°œà±à°²à±à°²à±‹ (in 3 days)";
            _logger.LogInformation("Sending {Wave}-day exam reminder for {Subject} to {Count} students",
                is1Day ? 1 : 3, exam.SubjectName, students.Count);

            foreach (var student in students)
            {
                try
                {
                    await _vapiService.InitiateOutboundCallAsync(student, school, CallType.ExamReminder, null);
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Exam reminder call failed for student {Id}", student.Id);
                }
            }

            if (is3Day) exam.Reminder3DaySent = true;
            if (is1Day) exam.Reminder1DaySent = true;
            exam.UpdatedAt = DateTime.UtcNow;
            _uow.ExamSchedules.Update(exam);
            await _uow.SaveChangesAsync();
        }
    }

    private static ExamScheduleDto Map(ExamSchedule e) => new()
    {
        Id = e.Id, SubjectName = e.SubjectName, ExamType = e.ExamType,
        ExamDate = e.ExamDate, Class = e.Class, Section = e.Section, Notes = e.Notes,
        Reminder3DaySent = e.Reminder3DaySent, Reminder1DaySent = e.Reminder1DaySent,
        IsActive = e.IsActive, CreatedAt = e.CreatedAt
    };
}
