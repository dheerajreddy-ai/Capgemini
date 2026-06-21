using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EduVoice.API.Controllers;

/// <summary>
/// Receives inbound WhatsApp messages from Twilio and replies via TwiML.
/// Configure Twilio Sandbox/WhatsApp number webhook to POST to /api/whatsapp/inbound.
/// </summary>
[ApiController]
[Route("api/whatsapp")]
[AllowAnonymous]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppBotService _botService;
    private readonly IConfiguration _configuration;

    public WhatsAppController(IWhatsAppBotService botService, IConfiguration configuration)
    {
        _botService = botService;
        _configuration = configuration;
    }

    /// <summary>
    /// Twilio webhook — receives inbound WhatsApp messages (form-encoded).
    /// Twilio sends: From, To, Body, NumMedia, MediaUrl0, etc.
    /// </summary>
    [HttpPost("inbound")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Inbound([FromForm] TwilioWhatsAppWebhook form)
    {
        var from = form.From ?? string.Empty;
        var to   = form.To   ?? string.Empty;
        var body = form.Body ?? string.Empty;

        var reply = await _botService.HandleInboundMessageAsync(from, to, body);

        // Respond with TwiML so Twilio sends the message back immediately
        var twiml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <Response>
                <Message><![CDATA[{reply}]]></Message>
            </Response>
            """;

        return Content(twiml, "text/xml");
    }
}

public class TwilioWhatsAppWebhook
{
    public string? From { get; set; }
    public string? To   { get; set; }
    public string? Body { get; set; }
    public string? NumMedia { get; set; }
    public string? MediaUrl0 { get; set; }
    public string? MediaContentType0 { get; set; }
}
