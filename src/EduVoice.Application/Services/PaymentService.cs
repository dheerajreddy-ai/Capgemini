using EduVoice.Application.Common;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITwilioService _twilioService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IUnitOfWork unitOfWork, ITwilioService twilioService, ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _twilioService = twilioService;
        _logger = logger;
    }

    public async Task<ApiResponse> SendPaymentLinkAsync(Guid schoolId, Guid studentId)
    {
        try
        {
            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && s.Id == studentId).FirstOrDefaultAsync();
            if (student == null) return ApiResponse.Fail("Student not found", "NOT_FOUND");

            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null) return ApiResponse.Fail("School not found", "NOT_FOUND");

            if (string.IsNullOrEmpty(school.UpiId))
                return ApiResponse.Fail("School UPI ID not configured. Add it in Settings → Voice.", "NO_UPI");

            if (student.PendingFees <= 0)
                return ApiResponse.Fail("No pending fees for this student", "NO_PENDING_FEES");

            var link = PaymentLinkHelper.GenerateUpiLink(school.UpiId, school.Name,
                student.PendingFees, $"{student.FirstName} {student.LastName}");

            student.PaymentLink = link;
            student.PaymentLinkGeneratedAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            var msg = PaymentLinkHelper.FeeReminderWhatsApp(student.ParentName,
                $"{student.FirstName} {student.LastName}", school.Name,
                student.PendingFees, student.FeesDueDate, link);

            await _twilioService.SendWhatsAppAsync(student.ParentPhone, msg);
            return ApiResponse.Ok("Payment link sent via WhatsApp");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment link for student {StudentId}", studentId);
            return ApiResponse.Fail("Failed to send payment link", "ERROR");
        }
    }

    public async Task<ApiResponse> ConfirmPaymentAsync(Guid schoolId, Guid studentId, decimal amountPaid)
    {
        try
        {
            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && s.Id == studentId).FirstOrDefaultAsync();
            if (student == null) return ApiResponse.Fail("Student not found", "NOT_FOUND");

            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);

            student.PaidFees += amountPaid;
            student.PendingFees = Math.Max(0, student.TotalFees - student.PaidFees);
            student.FeesStatus = student.PendingFees == 0 ? FeesStatus.Paid : FeesStatus.Partial;
            student.LastPaymentDate = DateTime.UtcNow;
            student.HasFeeExtension = false;
            student.FeeExtensionUntil = null;
            student.PaymentLink = null;
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            if (school != null)
            {
                var msg = PaymentLinkHelper.PaymentConfirmedWhatsApp(student.ParentName,
                    $"{student.FirstName} {student.LastName}", school.Name, amountPaid);
                _ = Task.Run(() => _twilioService.SendWhatsAppAsync(student.ParentPhone, msg));
            }

            return ApiResponse.Ok("Payment confirmed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment for student {StudentId}", studentId);
            return ApiResponse.Fail("Failed to confirm payment", "ERROR");
        }
    }

    public async Task<ApiResponse> RaiseDisputeAsync(Guid schoolId, Guid studentId, string note)
    {
        try
        {
            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && s.Id == studentId).FirstOrDefaultAsync();
            if (student == null) return ApiResponse.Fail("Student not found", "NOT_FOUND");

            student.HasFeeDispute = true;
            student.FeeDisputeNote = note;
            student.FeeDisputeRaisedAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Dispute recorded. Automated fee reminders paused for this student.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error raising dispute for student {StudentId}", studentId);
            return ApiResponse.Fail("Failed to raise dispute", "ERROR");
        }
    }

    public async Task<ApiResponse> ResolveDisputeAsync(Guid schoolId, Guid studentId)
    {
        try
        {
            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && s.Id == studentId).FirstOrDefaultAsync();
            if (student == null) return ApiResponse.Fail("Student not found", "NOT_FOUND");

            student.HasFeeDispute = false;
            student.FeeDisputeNote = null;
            student.FeeDisputeRaisedAt = null;
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Dispute resolved. Fee reminders re-enabled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving dispute for student {StudentId}", studentId);
            return ApiResponse.Fail("Failed to resolve dispute", "ERROR");
        }
    }

    public async Task<ApiResponse> GrantExtensionAsync(Guid schoolId, Guid studentId, int extraDays)
    {
        try
        {
            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && s.Id == studentId).FirstOrDefaultAsync();
            if (student == null) return ApiResponse.Fail("Student not found", "NOT_FOUND");

            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            var newDeadline = DateTime.UtcNow.AddDays(extraDays);

            student.HasFeeExtension = true;
            student.FeeExtensionUntil = newDeadline;
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            if (school != null)
            {
                var msg = PaymentLinkHelper.ExtensionGrantedWhatsApp(student.ParentName,
                    $"{student.FirstName} {student.LastName}", school.Name, newDeadline);
                _ = Task.Run(() => _twilioService.SendWhatsAppAsync(student.ParentPhone, msg));
            }

            return ApiResponse.Ok($"Extension granted until {newDeadline:dd MMM yyyy}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting extension for student {StudentId}", studentId);
            return ApiResponse.Fail("Failed to grant extension", "ERROR");
        }
    }
}
