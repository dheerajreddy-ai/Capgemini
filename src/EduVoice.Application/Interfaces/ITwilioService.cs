namespace EduVoice.Application.Interfaces;

public interface ITwilioService
{
    Task SendWhatsAppAsync(string toPhone, string message);
    Task<string> BuyPhoneNumberAsync(string areaCode);
    string ValidateIndianPhoneNumber(string phone);
}
