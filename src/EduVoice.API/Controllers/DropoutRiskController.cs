using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/dropout-risk")]
[Authorize]
[EnableRateLimiting("api")]
public class DropoutRiskController : ControllerBase
{
    private readonly IDropoutRiskService _service;

    public DropoutRiskController(IDropoutRiskService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetSummary([FromQuery] string? level = null)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.GetRiskSummaryAsync(schoolId.Value, level);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Manual trigger for recalculation (admin only, e.g. after bulk mark import).</summary>
    [HttpPost("recalculate")]
    public async Task<IActionResult> Recalculate()
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        await _service.RecalculateAllAsync();
        return Ok(new { success = true, message = "Risk scores recalculated." });
    }

    private Guid? GetSchoolId()
    {
        var claim = HttpContext.Items["SchoolId"]?.ToString() ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
