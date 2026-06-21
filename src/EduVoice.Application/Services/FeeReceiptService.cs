using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class FeeReceiptService : IFeeReceiptService
{
    private readonly ITwilioService _twilioService;
    private readonly ILogger<FeeReceiptService> _logger;

    public FeeReceiptService(ITwilioService twilioService, ILogger<FeeReceiptService> logger)
    {
        _twilioService = twilioService;
        _logger = logger;
    }

    public async Task SendReceiptAsync(Student student, School school, decimal amountPaid, string receiptNumber)
    {
        try
        {
            var studentName = $"{student.FirstName} {student.LastName}".Trim();
            var remaining = Math.Max(0, student.TotalFees - student.PaidFees);
            var statusEmoji = remaining == 0 ? "✅ పూర్తిగా చెల్లించారు! (FULLY PAID)" : $"⚠️ మిగిలిన బకాయి: ₹{remaining:N0}";
            var now = DateTime.Now; // IST local time

            var receipt = $"""
                🧾 *FEE RECEIPT — {school.Name}*
                ━━━━━━━━━━━━━━━━━━━━━━
                రసీదు నంబర్: *{receiptNumber}*
                తేదీ: {now:dd MMM yyyy, hh:mm tt}
                ━━━━━━━━━━━━━━━━━━━━━━
                👤 విద్యార్థి: *{studentName}*
                🏫 తరగతి: {student.Class ?? "N/A"} {student.Section ?? ""}
                📞 తల్లిదండ్రి: {student.ParentName}
                ━━━━━━━━━━━━━━━━━━━━━━
                💰 *చెల్లింపు వివరాలు*
                చెల్లించిన మొత్తం: *₹{amountPaid:N0}*
                మొత్తం ఫీజు: ₹{student.TotalFees:N0}
                ఇప్పటివరకు చెల్లించారు: ₹{student.PaidFees:N0}
                ━━━━━━━━━━━━━━━━━━━━━━
                {statusEmoji}
                ━━━━━━━━━━━━━━━━━━━━━━
                ధన్యవాదాలు! ఈ రసీదును భవిష్యత్తు సూచన కోసం భద్రపరచండి.
                (Thank you! Keep this receipt for future reference.)

                📞 {school.ContactPhone} | {school.ContactEmail}
                """;

            await _twilioService.SendWhatsAppAsync(student.ParentPhone, receipt);
            _logger.LogInformation("Fee receipt sent for student {StudentId}, receipt {Receipt}", student.Id, receiptNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send fee receipt for student {StudentId}", student.Id);
            // Non-fatal — don't throw, payment is already confirmed
        }
    }

    public static string GenerateReceiptNumber(Guid schoolId, DateTime date)
    {
        var prefix = $"EVR-{date:yyyyMM}-";
        var suffix = Math.Abs(schoolId.GetHashCode() % 100000).ToString("D5");
        return $"{prefix}{suffix}";
    }
}
