using EduVoice.Application.DTOs.Students;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/students")]
[Authorize]
[EnableRateLimiting("api")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? classFilter = null,
        [FromQuery] string? section = null,
        [FromQuery] FeesStatus? feesStatus = null,
        [FromQuery] string? sortBy = null)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _studentService.GetStudentsAsync(schoolId.Value, page, pageSize,
            search, classFilter, section, feesStatus, sortBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStudent(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _studentService.GetStudentByIdAsync(schoolId.Value, id);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _studentService.CreateStudentAsync(schoolId.Value, request);
        return result.Success ? CreatedAtAction(nameof(GetStudent), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _studentService.UpdateStudentAsync(schoolId.Value, id, request);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _studentService.DeleteStudentAsync(schoolId.Value, id);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("import")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> ImportStudents(IFormFile file)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return BadRequest("Only Excel files (.xlsx, .xls) are allowed");

        var result = await _studentService.ImportFromExcelAsync(schoolId.Value, file);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportStudents()
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var bytes = await _studentService.ExportToExcelAsync(schoolId.Value);
        var fileName = $"students-{DateTime.UtcNow:yyyy-MM-dd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private Guid? GetSchoolId()
    {
        var schoolIdClaim = HttpContext.Items["SchoolId"]?.ToString()
            ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(schoolIdClaim, out var id) ? id : null;
    }
}
