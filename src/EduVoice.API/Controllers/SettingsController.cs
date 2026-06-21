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

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
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

    private Guid? GetSchoolId()
    {
        var schoolIdClaim = HttpContext.Items["SchoolId"]?.ToString()
            ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(schoolIdClaim, out var id) ? id : null;
    }
}
