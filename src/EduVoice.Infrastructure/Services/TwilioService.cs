using EduVoice.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace EduVoice.Infrastructure.Services;

public class TwilioService : ITwilioService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioService> _logger;

    public TwilioService(IConfiguration configuration, ILogger<TwilioService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var accountSid = configuration["TWILIO_ACCOUNT_SID"];
        var authToken = configuration["TWILIO_AUTH_TOKEN"];

        if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
            TwilioClient.Init(accountSid, authToken);
    }

    public async Task SendWhatsAppAsync(string toPhone, string message)
    {
        try
        {
            var from = _configuration["TWILIO_WHATSAPP_FROM"] ?? "+14155238886";
            var normalizedPhone = ValidateIndianPhoneNumber(toPhone);
            var toWhatsApp = $"whatsapp:{normalizedPhone}";
            var fromWhatsApp = $"whatsapp:{from}";

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(fromWhatsApp),
                to: new PhoneNumber(toWhatsApp)
            );

            _logger.LogInformation("WhatsApp message sent to {Phone}, SID: {Sid}", normalizedPhone, messageResource.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {Phone}", toPhone);
            throw;
        }
    }

    public async Task SendWhatsAppWithMediaAsync(string toPhone, string message, string mediaUrl)
    {
        try
        {
            var from = _configuration["TWILIO_WHATSAPP_FROM"] ?? "+14155238886";
            var normalizedPhone = ValidateIndianPhoneNumber(toPhone);

            var msg = await MessageResource.CreateAsync(
                body: message,
                mediaUrl: [new Uri(mediaUrl)],
                from: new PhoneNumber($"whatsapp:{from}"),
                to: new PhoneNumber($"whatsapp:{normalizedPhone}")
            );
            _logger.LogInformation("WhatsApp media sent to {Phone}, SID: {Sid}", normalizedPhone, msg.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp media to {Phone}", toPhone);
            throw;
        }
    }

    public async Task<string> BuyPhoneNumberAsync(string areaCode)
    {
        try
        {
            var availableNumbers = await Twilio.Rest.Api.V2010.Account.AvailablePhoneNumberCountry.LocalResource.ReadAsync(
                pathCountryCode: "IN",
                areaCode: int.TryParse(areaCode, out var ac) ? ac : null
            );

            var list = availableNumbers.ToList();
            if (!list.Any())
                throw new Exception("No phone numbers available for the specified area code");

            var purchased = await Twilio.Rest.Api.V2010.Account.IncomingPhoneNumberResource.CreateAsync(
                phoneNumber: list.First().PhoneNumber?.ToString()
            );

            return purchased.PhoneNumber.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to buy phone number for area code {AreaCode}", areaCode);
            throw;
        }
    }

    public string ValidateIndianPhoneNumber(string phone)
    {
        var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

        if (cleaned.StartsWith("+91") && cleaned.Length == 13)
            return cleaned;

        if (cleaned.StartsWith("91") && cleaned.Length == 12)
            return $"+{cleaned}";

        if (cleaned.StartsWith("0") && cleaned.Length == 11)
            return $"+91{cleaned[1..]}";

        if (cleaned.Length == 10 && cleaned.All(char.IsDigit))
            return $"+91{cleaned}";

        return cleaned;
    }
}
