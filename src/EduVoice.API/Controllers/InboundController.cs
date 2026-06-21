using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/inbound")]
[AllowAnonymous]
[EnableRateLimiting("auth")]
public class InboundController : ControllerBase
{
    private readonly IInboundService _inboundService;

    public InboundController(IInboundService inboundService)
    {
        _inboundService = inboundService;
    }

    [HttpPost("portal/otp")]
    public async Task<IActionResult> SendOtp([FromBody] PortalOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.SubDomain))
            return BadRequest(new { message = "Phone and subDomain are required" });

        var result = await _inboundService.SendPortalOtpAsync(request.Phone, request.SubDomain);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("portal/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] PortalVerifyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Otp) || string.IsNullOrWhiteSpace(request.SubDomain))
            return BadRequest(new { message = "Phone, otp and subDomain are required" });

        var result = await _inboundService.VerifyPortalOtpAsync(request.Phone, request.Otp, request.SubDomain);
        if (!result.Success)
        {
            var status = result.Code == "NOT_FOUND" || result.Code == "INVALID" ? 404 : 400;
            return StatusCode(status, result);
        }

        return Ok(result);
    }
}

public record PortalOtpRequest(string Phone, string SubDomain);
public record PortalVerifyRequest(string Phone, string Otp, string SubDomain);
