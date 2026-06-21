using EduVoice.Application.DTOs.Teacher;
using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher,SchoolAdmin,SuperAdmin")]
[EnableRateLimiting("api")]
public class TeacherController : ControllerBase
{
    private readonly ITeacherService _teacherService;

    public TeacherController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _teacherService.GetDashboardAsync(schoolId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("class-sections")]
    public async Task<IActionResult> GetClassSections()
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _teacherService.GetClassSectionsAsync(schoolId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents([FromQuery] string className, [FromQuery] string? section)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(className)) return BadRequest("className is required");

        var result = await _teacherService.GetStudentsAsync(schoolId.Value, className, section);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance(
        [FromQuery] DateTime date,
        [FromQuery] string className,
        [FromQuery] string? section)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(className)) return BadRequest("className is required");

        var result = await _teacherService.GetAttendanceAsync(schoolId.Value, date, className, section);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("attendance")]
    public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceRequest request)
    {
        var schoolId = GetSchoolId();
        var userId = GetUserId();
        if (schoolId == null || userId == null) return Unauthorized();

        var result = await _teacherService.MarkAttendanceAsync(schoolId.Value, userId.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("marks")]
    public async Task<IActionResult> UploadMarks([FromBody] UploadMarksRequest request)
    {
        var schoolId = GetSchoolId();
        var userId = GetUserId();
        if (schoolId == null || userId == null) return Unauthorized();

        var result = await _teacherService.UploadMarksAsync(schoolId.Value, userId.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("homework")]
    public async Task<IActionResult> AssignHomework([FromBody] AssignHomeworkRequest request)
    {
        var schoolId = GetSchoolId();
        var userId = GetUserId();
        if (schoolId == null || userId == null) return Unauthorized();

        var result = await _teacherService.AssignHomeworkAsync(schoolId.Value, userId.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetSchoolId()
    {
        var claim = HttpContext.Items["SchoolId"]?.ToString() ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst("userId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
