using EduVoice.Application.DTOs.ComplaintSla;
using EduVoice.Application.DTOs.Settings;
using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
[EnableRateLimiting("api")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly IComplaintSlaService _slaService;

    public SettingsController(ISettingsService settingsService, IComplaintSlaService slaService)
    {
        _settingsService = settingsService;
        _slaService = slaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _settingsService.GetSettingsAsync(schoolId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _settingsService.UpdateSettingsAsync(schoolId.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue("userId")!);
        var result = await _settingsService.ChangePasswordAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("complaint-sla")]
    public async Task<IActionResult> GetSlaConfigs()
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _slaService.GetConfigsAsync(schoolId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("complaint-sla")]
    public async Task<IActionResult> UpsertSlaConfig([FromBody] UpsertSlaConfigRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _slaService.UpsertConfigAsync(schoolId.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetSchoolId()
    {
        var schoolIdClaim = HttpContext.Items["SchoolId"]?.ToString()
            ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(schoolIdClaim, out var id) ? id : null;
    }
}
