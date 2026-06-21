using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/fee-collection")]
[Authorize]
[EnableRateLimiting("api")]
public class FeeCollectionController : ControllerBase
{
    private readonly IFeeCollectionService _service;

    public FeeCollectionController(IFeeCollectionService service) => _service = service;

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var schoolId = GetSchoolId();
        if (schoolId is null) return Unauthorized();
        var result = await _service.GetDashboardAsync(schoolId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetSchoolId()
    {
        var claim = HttpContext.Items["SchoolId"]?.ToString() ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
