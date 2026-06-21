using EduVoice.Application.DTOs.Broadcasts;
using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/broadcasts")]
[Authorize]
[EnableRateLimiting("api")]
public class BroadcastController : ControllerBase
{
    private readonly IBroadcastService _broadcastService;

    public BroadcastController(IBroadcastService broadcastService)
    {
        _broadcastService = broadcastService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();
        var result = await _broadcastService.GetBroadcastsAsync(schoolId.Value, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();
        var result = await _broadcastService.GetByIdAsync(schoolId.Value, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBroadcastRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();
        var userId = Guid.Parse(User.FindFirstValue("userId")!);
        var result = await _broadcastService.CreateAndSendAsync(schoolId.Value, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetSchoolId()
    {
        var claim = HttpContext.Items["SchoolId"]?.ToString() ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
