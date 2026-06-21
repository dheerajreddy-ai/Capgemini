using EduVoice.Application.DTOs.Campaigns;
using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/campaigns")]
[Authorize]
[EnableRateLimiting("api")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public CampaignsController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCampaigns([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _campaignService.GetCampaignsAsync(schoolId.Value, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCampaign(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _campaignService.GetCampaignByIdAsync(schoolId.Value, id);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var userId = Guid.Parse(User.FindFirstValue("userId")!);
        var result = await _campaignService.CreateCampaignAsync(schoolId.Value, userId, request);
        return result.Success ? CreatedAtAction(nameof(GetCampaign), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/pause")]
    public async Task<IActionResult> PauseCampaign(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _campaignService.PauseCampaignAsync(schoolId.Value, id);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/resume")]
    public async Task<IActionResult> ResumeCampaign(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _campaignService.ResumeCampaignAsync(schoolId.Value, id);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelCampaign(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _campaignService.CancelCampaignAsync(schoolId.Value, id);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetSchoolId()
    {
        var schoolIdClaim = HttpContext.Items["SchoolId"]?.ToString()
            ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(schoolIdClaim, out var id) ? id : null;
    }
}
