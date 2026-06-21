using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/calls")]
[EnableRateLimiting("api")]
public class CallsController : ControllerBase
{
    private readonly ICallService _callService;
    private readonly IConfiguration _configuration;

    public CallsController(ICallService callService, IConfiguration configuration)
    {
        _callService = callService;
        _configuration = configuration;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCalls(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] CallStatus? status = null,
        [FromQuery] CallType? type = null,
        [FromQuery] Guid? campaignId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _callService.GetCallsAsync(schoolId.Value, page, pageSize, status, type, campaignId, from, to);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetCall(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _callService.GetCallByIdAsync(schoolId.Value, id);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> VapiWebhook([FromBody] VapiWebhookPayload payload)
    {
        var webhookSecret = _configuration["VAPI_WEBHOOK_SECRET"];
        if (!string.IsNullOrEmpty(webhookSecret))
        {
            var header = Request.Headers["X-Vapi-Secret"].FirstOrDefault();
            if (header != webhookSecret)
                return Unauthorized(new { message = "Invalid webhook secret" });
        }

        await _callService.ProcessVapiWebhookAsync(payload);
        return Ok(new { received = true });
    }

    [HttpPost("{id:guid}/retry")]
    [Authorize]
    public async Task<IActionResult> RetryCall(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _callService.RetryCallAsync(schoolId.Value, id);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetSchoolId()
    {
        var schoolIdClaim = HttpContext.Items["SchoolId"]?.ToString()
            ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(schoolIdClaim, out var id) ? id : null;
    }
}
