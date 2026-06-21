using System.Security.Cryptography;
using System.Text;
using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Portal;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class InboundService : IInboundService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITwilioService _twilioService;
    private readonly ILogger<InboundService> _logger;

    private const int OtpValidMinutes = 10;

    public InboundService(IUnitOfWork unitOfWork, ITwilioService twilioService, ILogger<InboundService> logger)
    {
        _unitOfWork = unitOfWork;
        _twilioService = twilioService;
        _logger = logger;
    }

    public async Task<ApiResponse> SendPortalOtpAsync(string phone, string subDomain)
    {
        try
        {
            var normalizedPhone = NormalizePhone(phone);

            var school = await _unitOfWork.Schools.Query()
                .Where(s => s.SubDomain == subDomain && s.IsActive)
                .FirstOrDefaultAsync();

            if (school == null)
                return ApiResponse.Fail("School not found", "NOT_FOUND");

            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == school.Id && !s.IsDeleted
                    && (s.ParentPhone == normalizedPhone || s.ParentPhone == phone))
                .FirstOrDefaultAsync();

            if (student == null)
                return ApiResponse.Fail("No student found for this mobile number", "NOT_FOUND");

            var otp = GenerateOtp();
            student.PortalOtpHash = HashOtp(otp);
            student.PortalOtpExpiresAt = DateTime.UtcNow.AddMinutes(OtpValidMinutes);
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            var message = $"Your EduVoice parent portal OTP is: *{otp}*\n" +
                          $"Valid for {OtpValidMinutes} minutes. Do not share this with anyone.\n" +
                          $"- {school.Name}";

            await _twilioService.SendWhatsAppAsync(normalizedPhone, message);

            return ApiResponse.Ok("OTP sent via WhatsApp");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending portal OTP for phone {Phone}", phone);
            return ApiResponse.Fail("Failed to send OTP", "ERROR");
        }
    }

    public async Task<ApiResponse<PortalDataDto>> VerifyPortalOtpAsync(string phone, string otp, string subDomain)
    {
        try
        {
            var normalizedPhone = NormalizePhone(phone);

            var school = await _unitOfWork.Schools.Query()
                .Where(s => s.SubDomain == subDomain && s.IsActive)
                .FirstOrDefaultAsync();

            if (school == null)
                return ApiResponse<PortalDataDto>.Fail("School not found", "NOT_FOUND");

            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == school.Id && !s.IsDeleted
                    && (s.ParentPhone == normalizedPhone || s.ParentPhone == phone))
                .FirstOrDefaultAsync();

            if (student == null)
                return ApiResponse<PortalDataDto>.Fail("Invalid mobile number", "INVALID");

            if (student.PortalOtpHash == null || student.PortalOtpExpiresAt == null)
                return ApiResponse<PortalDataDto>.Fail("No OTP requested. Please request a new OTP.", "NO_OTP");

            if (DateTime.UtcNow > student.PortalOtpExpiresAt)
                return ApiResponse<PortalDataDto>.Fail("OTP has expired. Please request a new one.", "OTP_EXPIRED");

            if (HashOtp(otp) != student.PortalOtpHash)
                return ApiResponse<PortalDataDto>.Fail("Invalid OTP", "INVALID_OTP");

            // Clear OTP after successful verification
            student.PortalOtpHash = null;
            student.PortalOtpExpiresAt = null;
            student.LastPortalAccessAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            // Fetch recent calls
            var recentCalls = await _unitOfWork.Calls.Query()
                .Where(c => c.StudentId == student.Id && c.SchoolId == school.Id)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new PortalCallDto
                {
                    Type = c.Type.ToString(),
                    Status = c.Status.ToString(),
                    AiSummary = c.AiSummary,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            // Fetch open complaints
            var openComplaints = await _unitOfWork.Complaints.Query()
                .Where(c => c.StudentId == student.Id && c.SchoolId == school.Id
                    && c.Status != ComplaintStatus.Resolved && c.Status != ComplaintStatus.Closed)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new PortalComplaintDto
                {
                    Summary = c.Summary,
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            var data = new PortalDataDto
            {
                StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                Class = student.Class,
                FeesStatus = student.FeesStatus.ToString(),
                PendingFees = student.PendingFees,
                TotalFees = student.TotalFees,
                PaidFees = student.PaidFees,
                FeesDueDate = student.FeesDueDate,
                PaymentLink = student.PendingFees > 0 ? student.PaymentLink : null,
                AttendancePercentage = student.AttendancePercentage,
                Percentage = student.Percentage,
                Grade = student.Grade,
                MathMarks = student.MathMarks,
                ScienceMarks = student.ScienceMarks,
                EnglishMarks = student.EnglishMarks,
                TeluguMarks = student.TeluguMarks,
                SocialMarks = student.SocialMarks,
                RecentCalls = recentCalls,
                OpenComplaints = openComplaints
            };

            return ApiResponse<PortalDataDto>.Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying portal OTP for phone {Phone}", phone);
            return ApiResponse<PortalDataDto>.Fail("Failed to verify OTP", "ERROR");
        }
    }

    private static string GenerateOtp()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var num = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return num.ToString("D6");
    }

    private static string HashOtp(string otp)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
        return Convert.ToHexString(bytes);
    }

    private static string NormalizePhone(string phone)
    {
        var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
        if (cleaned.StartsWith("+91") && cleaned.Length == 13) return cleaned;
        if (cleaned.StartsWith("91") && cleaned.Length == 12) return $"+{cleaned}";
        if (cleaned.StartsWith("0") && cleaned.Length == 11) return $"+91{cleaned[1..]}";
        if (cleaned.Length == 10 && cleaned.All(char.IsDigit)) return $"+91{cleaned}";
        return cleaned;
    }
}
