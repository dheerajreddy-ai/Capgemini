using EduVoice.Application.DTOs.Admin;
using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin")]
[EnableRateLimiting("api")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("schools")]
    public async Task<IActionResult> GetSchools(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _adminService.GetSchoolsAsync(page, pageSize, search);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("schools")]
    public async Task<IActionResult> CreateSchool([FromBody] CreateSchoolRequest request)
    {
        var result = await _adminService.CreateSchoolAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("schools/{id:guid}")]
    public async Task<IActionResult> UpdateSchool(Guid id, [FromBody] CreateSchoolRequest request)
    {
        var result = await _adminService.UpdateSchoolAsync(id, request);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("schools/{id:guid}")]
    public async Task<IActionResult> DeleteSchool(Guid id)
    {
        var result = await _adminService.DeleteSchoolAsync(id);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("schools/{id:guid}/stats")]
    public async Task<IActionResult> GetSchoolStats(Guid id)
    {
        var result = await _adminService.GetSchoolStatsAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
