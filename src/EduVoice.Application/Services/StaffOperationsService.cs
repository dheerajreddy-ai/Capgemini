using EduVoice.Application.Common;
using EduVoice.Application.DTOs.StaffOperations;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class StaffOperationsService : IStaffOperationsService
{
    private readonly IUnitOfWork _uow;
    private readonly ITwilioService _twilio;
    private readonly ILogger<StaffOperationsService> _logger;

    public StaffOperationsService(IUnitOfWork uow, ITwilioService twilio,
        ILogger<StaffOperationsService> logger)
    {
        _uow = uow;
        _twilio = twilio;
        _logger = logger;
    }

    public async Task<ApiResponse<StaffAbsenceDto>> LogAbsenceAsync(Guid schoolId, Guid userId, LogAbsenceRequest req)
    {
        var school = await _uow.Schools.GetByIdAsync(schoolId);
        if (school is null) return ApiResponse<StaffAbsenceDto>.Fail("School not found", "NOT_FOUND");

        var absence = new StaffAbsence
        {
            Id = Guid.NewGuid(),
            SchoolId = schoolId,
            TeacherName = req.TeacherName,
            TeacherPhone = req.TeacherPhone,
            SubstituteTeacherName = req.SubstituteTeacherName,
            SubstituteTeacherPhone = req.SubstituteTeacherPhone,
            AffectedClass = req.AffectedClass,
            AffectedSection = req.AffectedSection,
            AbsenceDate = req.AbsenceDate.Date,
            Notes = req.Notes,
            CreatedByUserId = userId,
        };

        // Alert substitute teacher via WhatsApp
        if (!string.IsNullOrWhiteSpace(req.SubstituteTeacherPhone)
            && !string.IsNullOrWhiteSpace(req.SubstituteTeacherName))
        {
            var classInfo = FormatClass(req.AffectedClass, req.AffectedSection);
            var subMsg = $"""
                ðŸ”” *Substitute Assignment â€” {school.Name}*

                Dear {req.SubstituteTeacherName},

                You have been assigned as *substitute teacher*{(classInfo != "" ? $" for Class {classInfo}" : "")} on *{req.AbsenceDate:dd MMM yyyy}*.

                {(string.IsNullOrWhiteSpace(req.TeacherName) ? "" : $"Regular teacher: {req.TeacherName} is absent today.")}
                {(string.IsNullOrWhiteSpace(req.Notes) ? "" : $"\nNote: {req.Notes}")}

                Please report to the school office for the timetable.

                â€” {school.Name}
                """;
            try
            {
                await _twilio.SendWhatsAppAsync(req.SubstituteTeacherPhone, subMsg);
                absence.SubstituteAlertSent = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Substitute alert failed for absence {Id}", absence.Id);
            }
        }

        // Notify parents of affected class
        if (req.NotifyParents && !string.IsNullOrWhiteSpace(req.AffectedClass))
        {
            var classInfo = FormatClass(req.AffectedClass, req.AffectedSection);
            var students = await _uow.Students.Query()
                .Where(s => s.SchoolId == schoolId && !s.IsDeleted
                    && s.Class == req.AffectedClass
                    && (req.AffectedSection == null || s.Section == req.AffectedSection))
                .ToListAsync();

            int notified = 0;
            foreach (var student in students)
            {
                var phone = student.ParentWhatsApp ?? student.ParentPhone;
                if (string.IsNullOrWhiteSpace(phone)) continue;

                var parentMsg = $"""
                    ðŸ“¢ *Class Notice â€” {school.Name}*

                    Dear {student.ParentName},

                    The regular teacher for Class {classInfo} is absent today (*{req.AbsenceDate:dd MMM yyyy}*). A substitute teacher has been arranged.

                    Your child's studies will not be affected.
                    {(string.IsNullOrWhiteSpace(req.Notes) ? "" : $"\nNote: {req.Notes}")}

                    â€” {school.Name}
                    """;
                try
                {
                    await _twilio.SendWhatsAppAsync(phone, parentMsg);
                    notified++;
                    await Task.Delay(1100);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Parent absence notification failed for student {Id}", student.Id);
                }
            }

            absence.ParentNotificationSent = notified > 0;
        }

        await _uow.StaffAbsences.AddAsync(absence);
        await _uow.SaveChangesAsync();

        return ApiResponse<StaffAbsenceDto>.Ok(MapAbsenceDto(absence));
    }

    public async Task<ApiResponse<List<StaffAbsenceDto>>> GetAbsencesAsync(Guid schoolId, DateTime? date = null)
    {
        var query = _uow.StaffAbsences.Query()
            .Where(a => a.SchoolId == schoolId);

        if (date.HasValue)
            query = query.Where(a => a.AbsenceDate.Date == date.Value.Date);
        else
            query = query.Where(a => a.AbsenceDate >= DateTime.UtcNow.AddDays(-30));

        var list = await query.OrderByDescending(a => a.AbsenceDate).ToListAsync();
        return ApiResponse<List<StaffAbsenceDto>>.Ok(list.Select(MapAbsenceDto).ToList());
    }

    public async Task<ApiResponse<PtmScheduleDto>> SchedulePtmAsync(Guid schoolId, Guid userId, CreatePtmRequest req)
    {
        if (req.PtmDate.Date < DateTime.UtcNow.Date)
            return ApiResponse<PtmScheduleDto>.Fail("PTM date cannot be in the past", "INVALID_DATE");

        var ptm = new PtmSchedule
        {
            Id = Guid.NewGuid(),
            SchoolId = schoolId,
            Title = req.Title,
            PtmDate = req.PtmDate.Date,
            Class = req.Class,
            Section = req.Section,
            Notes = req.Notes,
            CreatedByUserId = userId,
        };

        await _uow.PtmSchedules.AddAsync(ptm);
        await _uow.SaveChangesAsync();

        return ApiResponse<PtmScheduleDto>.Ok(MapPtmDto(ptm));
    }

    public async Task<ApiResponse<List<PtmScheduleDto>>> GetPtmSchedulesAsync(Guid schoolId)
    {
        var list = await _uow.PtmSchedules.Query()
            .Where(p => p.SchoolId == schoolId && p.IsActive)
            .OrderBy(p => p.PtmDate)
            .ToListAsync();
        return ApiResponse<List<PtmScheduleDto>>.Ok(list.Select(MapPtmDto).ToList());
    }

    public async Task<ApiResponse> DeletePtmAsync(Guid schoolId, Guid ptmId)
    {
        var ptm = await _uow.PtmSchedules.Query()
            .FirstOrDefaultAsync(p => p.Id == ptmId && p.SchoolId == schoolId);
        if (ptm is null) return ApiResponse.Fail("PTM not found", "NOT_FOUND");

        ptm.IsActive = false;
        _uow.PtmSchedules.Update(ptm);
        await _uow.SaveChangesAsync();
        return ApiResponse.Ok("PTM cancelled");
    }

    public async Task ProcessPtmRemindersAsync()
    {
        var today = DateTime.UtcNow.Date;
        var day3 = today.AddDays(3);
        var day1 = today.AddDays(1);

        var upcoming = await _uow.PtmSchedules.Query()
            .Where(p => p.IsActive
                && ((!p.Reminder3DaySent && p.PtmDate.Date == day3)
                    || (!p.Reminder1DaySent && p.PtmDate.Date == day1)))
            .Include(p => p.School)
            .ToListAsync();

        foreach (var ptm in upcoming)
        {
            var is3Day = ptm.PtmDate.Date == day3 && !ptm.Reminder3DaySent;
            var is1Day = ptm.PtmDate.Date == day1 && !ptm.Reminder1DaySent;
            if (!is3Day && !is1Day) continue;

            var classFilter = ptm.Class;
            var sectionFilter = ptm.Section;

            var studentsQuery = _uow.Students.Query()
                .Where(s => s.SchoolId == ptm.SchoolId && !s.IsDeleted);
            if (!string.IsNullOrWhiteSpace(classFilter))
                studentsQuery = studentsQuery.Where(s => s.Class == classFilter);
            if (!string.IsNullOrWhiteSpace(sectionFilter))
                studentsQuery = studentsQuery.Where(s => s.Section == sectionFilter);

            var students = await studentsQuery.ToListAsync();
            var classInfo = FormatClass(ptm.Class, ptm.Section);
            var daysLabel = is3Day ? "3 days" : "tomorrow";

            foreach (var student in students)
            {
                var phone = student.ParentWhatsApp ?? student.ParentPhone;
                if (string.IsNullOrWhiteSpace(phone)) continue;

                var msg = $"""
                    ðŸ“… *PTM Reminder â€” {ptm.School.Name}*

                    Dear {student.ParentName},

                    This is a reminder that the *Parent-Teacher Meeting* â€” *{ptm.Title}*{(classInfo != "" ? $" for Class {classInfo}" : "")} â€” is scheduled *{daysLabel} from today* on *{ptm.PtmDate:dd MMM yyyy}* at school.

                    Please make it a point to attend.
                    {(string.IsNullOrWhiteSpace(ptm.Notes) ? "" : $"\nNote: {ptm.Notes}")}

                    â€” {ptm.School.Name}
                    """;
                try
                {
                    await _twilio.SendWhatsAppAsync(phone, msg);
                    await Task.Delay(1100);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "PTM reminder failed for student {Id}", student.Id);
                }
            }

            if (is3Day) ptm.Reminder3DaySent = true;
            if (is1Day) ptm.Reminder1DaySent = true;
            _uow.PtmSchedules.Update(ptm);
        }

        if (upcoming.Count > 0)
            await _uow.SaveChangesAsync();

        _logger.LogInformation("PtmReminders: processed {Count} PTM events", upcoming.Count);
    }

    private static string FormatClass(string? cls, string? section) =>
        cls == null ? "" : section == null ? cls : $"{cls} â€“ {section}";

    private static StaffAbsenceDto MapAbsenceDto(StaffAbsence a) => new()
    {
        Id = a.Id,
        TeacherName = a.TeacherName,
        TeacherPhone = a.TeacherPhone,
        SubstituteTeacherName = a.SubstituteTeacherName,
        SubstituteTeacherPhone = a.SubstituteTeacherPhone,
        AffectedClass = a.AffectedClass,
        AffectedSection = a.AffectedSection,
        AbsenceDate = a.AbsenceDate,
        Notes = a.Notes,
        SubstituteAlertSent = a.SubstituteAlertSent,
        ParentNotificationSent = a.ParentNotificationSent,
        CreatedAt = a.CreatedAt,
    };

    private static PtmScheduleDto MapPtmDto(PtmSchedule p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        PtmDate = p.PtmDate,
        Class = p.Class,
        Section = p.Section,
        Notes = p.Notes,
        Reminder3DaySent = p.Reminder3DaySent,
        Reminder1DaySent = p.Reminder1DaySent,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
    };
}
