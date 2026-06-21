using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/parent-engagement")]
[Authorize]
[EnableRateLimiting("api")]
public class ParentEngagementController : ControllerBase
{
    private readonly IParentEngagementService _service;

    public ParentEngagementController(IParentEngagementService service) => _service = service;

    [HttpPost("birthday-wishes")]
    public async Task<IActionResult> SendBirthdayWishes()
    {
        var result = await _service.SendBirthdayWishesAsync();
        return Ok(result);
    }

    [HttpPost("weekly-summaries")]
    public async Task<IActionResult> SendWeeklySummaries()
    {
        var result = await _service.SendWeeklySummariesAsync();
        return Ok(result);
    }
}
