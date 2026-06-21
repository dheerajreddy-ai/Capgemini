using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Instalments;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class InstalmentService : IInstalmentService
{
    private readonly IUnitOfWork _uow;
    private readonly ITwilioService _twilio;
    private readonly ILogger<InstalmentService> _logger;

    public InstalmentService(IUnitOfWork uow, ITwilioService twilio, ILogger<InstalmentService> logger)
    {
        _uow = uow;
        _twilio = twilio;
        _logger = logger;
    }

    public async Task<ApiResponse<InstalmentPlanDto>> CreatePlanAsync(Guid schoolId, Guid studentId, CreateInstalmentPlanRequest request)
    {
        if (request.InstalmentCount < 2 || request.InstalmentCount > 24)
            return ApiResponse<InstalmentPlanDto>.Fail("Instalment count must be between 2 and 24", "INVALID_COUNT");
        if (request.IntervalDays < 7)
            return ApiResponse<InstalmentPlanDto>.Fail("Interval must be at least 7 days", "INVALID_INTERVAL");
        if (request.FirstDueDate.Date < DateTime.UtcNow.Date)
            return ApiResponse<InstalmentPlanDto>.Fail("First due date cannot be in the past", "INVALID_DATE");

        var student = await _uow.Students.Query()
            .FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId && !s.IsDeleted);
        if (student is null) return ApiResponse<InstalmentPlanDto>.Fail("Student not found", "NOT_FOUND");
        if (student.PendingFees <= 0) return ApiResponse<InstalmentPlanDto>.Fail("Student has no pending fees", "NO_PENDING_FEES");

        // Remove any existing unpaid instalments
        var existing = await _uow.FeeInstalments.Query()
            .Where(f => f.StudentId == studentId && !f.IsPaid)
            .ToListAsync();
        foreach (var e in existing)
            await _uow.FeeInstalments.DeleteAsync(e);

        var perInstalment = Math.Round(student.PendingFees / request.InstalmentCount, 2);
        var remainder = student.PendingFees - perInstalment * (request.InstalmentCount - 1);

        var instalments = new List<FeeInstalment>();
        for (int i = 0; i < request.InstalmentCount; i++)
        {
            instalments.Add(new FeeInstalment
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                StudentId = studentId,
                InstalmentNumber = i + 1,
                Amount = i == request.InstalmentCount - 1 ? remainder : perInstalment,
                DueDate = request.FirstDueDate.AddDays(i * request.IntervalDays),
            });
        }

        foreach (var inst in instalments)
            await _uow.FeeInstalments.AddAsync(inst);
        await _uow.SaveChangesAsync();

        return ApiResponse<InstalmentPlanDto>.Ok(BuildPlanDto(student, instalments));
    }

    public async Task<ApiResponse<InstalmentPlanDto>> GetPlanAsync(Guid schoolId, Guid studentId)
    {
        var student = await _uow.Students.Query()
            .FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId && !s.IsDeleted);
        if (student is null) return ApiResponse<InstalmentPlanDto>.Fail("Student not found", "NOT_FOUND");

        var instalments = await _uow.FeeInstalments.Query()
            .Where(f => f.StudentId == studentId && f.SchoolId == schoolId)
            .OrderBy(f => f.InstalmentNumber)
            .ToListAsync();

        return ApiResponse<InstalmentPlanDto>.Ok(BuildPlanDto(student, instalments));
    }

    public async Task<ApiResponse<FeeInstalmentDto>> MarkPaidAsync(Guid schoolId, Guid instalmentId)
    {
        var instalment = await _uow.FeeInstalments.Query()
            .Include(f => f.Student)
            .FirstOrDefaultAsync(f => f.Id == instalmentId && f.SchoolId == schoolId);
        if (instalment is null) return ApiResponse<FeeInstalmentDto>.Fail("Instalment not found", "NOT_FOUND");
        if (instalment.IsPaid) return ApiResponse<FeeInstalmentDto>.Fail("Instalment already marked paid", "ALREADY_PAID");

        instalment.IsPaid = true;
        instalment.PaidAt = DateTime.UtcNow;
        await _uow.FeeInstalments.UpdateAsync(instalment);

        // Update student fee balances
        var student = instalment.Student;
        student.PaidFees += instalment.Amount;
        student.PendingFees = Math.Max(0, student.PendingFees - instalment.Amount);
        student.LastPaymentDate = DateTime.UtcNow;

        // Check if all instalments are now paid
        var remaining = await _uow.FeeInstalments.Query()
            .CountAsync(f => f.StudentId == student.Id && !f.IsPaid);
        student.FeesStatus = remaining == 0 ? FeesStatus.Paid : FeesStatus.Partial;

        await _uow.Students.UpdateAsync(student);
        await _uow.SaveChangesAsync();

        return ApiResponse<FeeInstalmentDto>.Ok(MapToDto(instalment));
    }

    public async Task<ApiResponse> DeletePlanAsync(Guid schoolId, Guid studentId)
    {
        var unpaid = await _uow.FeeInstalments.Query()
            .Where(f => f.StudentId == studentId && f.SchoolId == schoolId && !f.IsPaid)
            .ToListAsync();
        if (unpaid.Count == 0) return ApiResponse.Fail("No active instalment plan found", "NOT_FOUND");

        foreach (var f in unpaid)
            await _uow.FeeInstalments.DeleteAsync(f);
        await _uow.SaveChangesAsync();

        return ApiResponse.Ok("Instalment plan removed");
    }

    public async Task CheckOverdueAndRemindAsync()
    {
        var today = DateTime.UtcNow.Date;

        var overdueInstalments = await _uow.FeeInstalments.Query()
            .Where(f => !f.IsPaid && f.DueDate.Date < today
                && (f.OverdueReminderSentAt == null
                    || f.OverdueReminderSentAt.Value.Date < today.AddDays(-3)))
            .Include(f => f.Student)
            .Include(f => f.School)
            .ToListAsync();

        foreach (var inst in overdueInstalments)
        {
            var student = inst.Student;
            var school = inst.School;
            var phone = student.ParentWhatsApp ?? student.ParentPhone;
            if (string.IsNullOrWhiteSpace(phone)) continue;

            var daysOverdue = (today - inst.DueDate.Date).Days;
            var msg = $"""
                📅 *Fee Instalment Due — {school.Name}*

                Dear Parent of *{student.FirstName} {student.LastName}* (Class {student.Class}),

                Instalment #{inst.InstalmentNumber} of ₹{inst.Amount:N0} was due on {inst.DueDate:dd MMM yyyy} and is now *{daysOverdue} day(s) overdue*.

                Please make the payment at the earliest to avoid any inconvenience.

                — EduVoice
                """;
            try
            {
                await _twilio.SendWhatsAppAsync(phone, msg);
                inst.OverdueReminderSentAt = DateTime.UtcNow;
                await _uow.FeeInstalments.UpdateAsync(inst);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed overdue reminder for instalment {Id}", inst.Id);
            }
        }

        if (overdueInstalments.Count > 0)
            await _uow.SaveChangesAsync();

        _logger.LogInformation("InstalmentReminder: processed {Count} overdue instalments", overdueInstalments.Count);
    }

    private static InstalmentPlanDto BuildPlanDto(Student student, List<FeeInstalment> instalments)
    {
        var today = DateTime.UtcNow.Date;
        var dtos = instalments.Select(MapToDto).ToList();
        return new InstalmentPlanDto
        {
            StudentId = student.Id,
            StudentName = $"{student.FirstName} {student.LastName}",
            TotalPending = student.PendingFees,
            TotalInstalments = instalments.Count,
            PaidInstalments = instalments.Count(i => i.IsPaid),
            OverdueInstalments = instalments.Count(i => !i.IsPaid && i.DueDate.Date < today),
            Instalments = dtos,
        };
    }

    private static FeeInstalmentDto MapToDto(FeeInstalment f)
    {
        var today = DateTime.UtcNow.Date;
        var isOverdue = !f.IsPaid && f.DueDate.Date < today;
        return new FeeInstalmentDto
        {
            Id = f.Id,
            InstalmentNumber = f.InstalmentNumber,
            Amount = f.Amount,
            DueDate = f.DueDate,
            IsPaid = f.IsPaid,
            PaidAt = f.PaidAt,
            IsOverdue = isOverdue,
            DaysOverdue = isOverdue ? (today - f.DueDate.Date).Days : 0,
        };
    }
}
