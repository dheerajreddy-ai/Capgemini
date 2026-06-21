namespace EduVoice.Application.Interfaces;

public interface IWhatsAppBotService
{
    /// <summary>
    /// Processes an inbound WhatsApp message from a parent.
    /// Returns the reply text to send back via TwiML.
    /// </summary>
    Task<string> HandleInboundMessageAsync(string fromPhone, string toPhone, string messageBody);
}
