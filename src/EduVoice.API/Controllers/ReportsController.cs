using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
[EnableRateLimiting("api")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    [HttpGet("student/{studentId:guid}")]
    public async Task<IActionResult> GetStudentReportCard(Guid studentId)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.GetStudentReportCardAsync(schoolId.Value, studentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("academic")]
    public async Task<IActionResult> GetAcademicReport([FromQuery] string? class_ = null)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.GetAcademicReportAsync(schoolId.Value, class_);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetSchoolId()
    {
        var claim = HttpContext.Items["SchoolId"]?.ToString() ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
