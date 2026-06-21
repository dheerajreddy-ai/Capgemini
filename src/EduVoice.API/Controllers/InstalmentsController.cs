using EduVoice.Application.DTOs.Instalments;
using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/students/{studentId:guid}/instalments")]
[Authorize]
[EnableRateLimiting("api")]
public class InstalmentsController : ControllerBase
{
    private readonly IInstalmentService _service;

    public InstalmentsController(IInstalmentService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetPlan(Guid studentId)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.GetPlanAsync(schoolId.Value, studentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlan(Guid studentId, [FromBody] CreateInstalmentPlanRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.CreatePlanAsync(schoolId.Value, studentId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete]
    public async Task<IActionResult> DeletePlan(Guid studentId)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.DeletePlanAsync(schoolId.Value, studentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{instalmentId:guid}/mark-paid")]
    public async Task<IActionResult> MarkPaid(Guid studentId, Guid instalmentId)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.MarkPaidAsync(schoolId.Value, instalmentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetSchoolId()
    {
        var claim = HttpContext.Items["SchoolId"]?.ToString() ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
