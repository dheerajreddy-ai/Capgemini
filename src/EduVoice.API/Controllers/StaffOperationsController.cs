using EduVoice.Application.DTOs.StaffOperations;
using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/staff-operations")]
[Authorize]
[EnableRateLimiting("api")]
public class StaffOperationsController : ControllerBase
{
    private readonly IStaffOperationsService _service;

    public StaffOperationsController(IStaffOperationsService service) => _service = service;

    // ---------- Absences ----------

    [HttpGet("absences")]
    public async Task<IActionResult> GetAbsences([FromQuery] DateTime? date = null)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.GetAbsencesAsync(schoolId.Value, date);
        return Ok(result);
    }

    [HttpPost("absences")]
    public async Task<IActionResult> LogAbsence([FromBody] LogAbsenceRequest request)
    {
        var schoolId = GetSchoolId();
        var userId = GetUserId();
        if (schoolId is null || userId is null) return Unauthorized();
        var result = await _service.LogAbsenceAsync(schoolId.Value, userId.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ---------- PTM ----------

    [HttpGet("ptm")]
    public async Task<IActionResult> GetPtmSchedules()
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.GetPtmSchedulesAsync(schoolId.Value);
        return Ok(result);
    }

    [HttpPost("ptm")]
    public async Task<IActionResult> SchedulePtm([FromBody] CreatePtmRequest request)
    {
        var schoolId = GetSchoolId();
        var userId = GetUserId();
        if (schoolId is null || userId is null) return Unauthorized();
        var result = await _service.SchedulePtmAsync(schoolId.Value, userId.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("ptm/{ptmId:guid}")]
    public async Task<IActionResult> DeletePtm(Guid ptmId)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.DeletePtmAsync(schoolId.Value, ptmId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    private Guid? GetSchoolId()
    {
        var claim = HttpContext.Items["SchoolId"]?.ToString() ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
