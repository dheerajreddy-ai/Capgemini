using EduVoice.Application.DTOs.Users;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[EnableRateLimiting("api")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "SchoolAdmin,SuperAdmin")]
    public async Task<IActionResult> GetUsers()
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _userService.GetUsersAsync(schoolId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [Authorize(Roles = "SchoolAdmin,SuperAdmin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (userRole != UserRole.SchoolAdmin.ToString() && userRole != UserRole.SuperAdmin.ToString())
            return Forbid();

        var result = await _userService.CreateUserAsync(schoolId.Value, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SchoolAdmin,SuperAdmin")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _userService.UpdateUserAsync(schoolId.Value, id, request);
        if (!result.Success && result.Code == "NOT_FOUND") return NotFound(result);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SchoolAdmin,SuperAdmin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();

        var result = await _userService.DeleteUserAsync(schoolId.Value, id);
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
