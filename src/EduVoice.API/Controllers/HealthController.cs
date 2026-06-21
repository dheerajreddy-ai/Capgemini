using EduVoice.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public HealthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Health()
    {
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
            return Ok(new
            {
                status = "healthy",
                database = "connected",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "unhealthy",
                database = "disconnected",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
