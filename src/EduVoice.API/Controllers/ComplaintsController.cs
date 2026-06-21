using EduVoice.Application.DTOs.Complaints;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize]
[EnableRateLimiting("api")]
public class ComplaintsController : ControllerBase
{
    private readonly IComplaintService _complaintService;

    public ComplaintsController(IComplaintService complaintService)
    {
        _complaintService = complaintService;
    }

    [HttpGet]
    public async Task<IActionResult> GetComplaints(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ComplaintStatus? status = null,
        [FromQuery] ComplaintCategory? category = null,
        [FromQuery] ComplaintPriority? priority = null)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _complaintService.GetComplaintsAsync(schoolId.Value, page, pageSize, status, category, priority);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetComplaint(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _complaintService.GetComplaintByIdAsync(schoolId.Value, id);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateComplaint(Guid id, [FromBody] UpdateComplaintRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _complaintService.UpdateComplaintAsync(schoolId.Value, id, request);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportComplaints()
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var bytes = await _complaintService.ExportToExcelAsync(schoolId.Value);
        var fileName = $"complaints-{DateTime.UtcNow:yyyy-MM-dd}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private Guid? GetSchoolId()
    {
        var schoolIdClaim = HttpContext.Items["SchoolId"]?.ToString()
            ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(schoolIdClaim, out var id) ? id : null;
    }
}
