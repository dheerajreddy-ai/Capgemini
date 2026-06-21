namespace EduVoice.Application.Interfaces;

public interface ITwilioService
{
    Task SendWhatsAppAsync(string toPhone, string message);
    Task SendWhatsAppWithMediaAsync(string toPhone, string message, string mediaUrl);
    Task<string> BuyPhoneNumberAsync(string areaCode);
    string ValidateIndianPhoneNumber(string phone);
}
