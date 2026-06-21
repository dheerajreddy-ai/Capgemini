using EduVoice.Domain.Entities;

namespace EduVoice.Application.Interfaces;

public interface IFeeReceiptService
{
    /// <summary>
    /// Generates a formatted WhatsApp fee receipt and sends it to the parent.
    /// Called automatically after payment confirmation.
    /// </summary>
    Task SendReceiptAsync(Student student, School school, decimal amountPaid, string receiptNumber);
}
