using EduVoice.Application.DTOs.ExamSchedules;
using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/exam-schedules")]
[Authorize]
[EnableRateLimiting("api")]
public class ExamScheduleController : ControllerBase
{
    private readonly IExamScheduleService _service;

    public ExamScheduleController(IExamScheduleService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.GetAllAsync(schoolId.Value, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.GetByIdAsync(schoolId.Value, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExamScheduleRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var userId = Guid.Parse(User.FindFirstValue("userId")!);
        var result = await _service.CreateAsync(schoolId.Value, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.DeleteAsync(schoolId.Value, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    private Guid? GetSchoolId()
    {
        var claim = HttpContext.Items["SchoolId"]?.ToString() ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
