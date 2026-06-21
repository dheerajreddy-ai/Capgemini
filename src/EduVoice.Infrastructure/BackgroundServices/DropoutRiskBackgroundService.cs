using EduVoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.BackgroundServices;

/// <summary>
/// Recalculates dropout risk scores for all students every Sunday at 7AM IST.
/// Alerts principal via WhatsApp if any student reaches Critical level.
/// </summary>
public class DropoutRiskBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DropoutRiskBackgroundService> _logger;

    private static readonly TimeZoneInfo Ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    public DropoutRiskBackgroundService(IServiceScopeFactory scopeFactory, ILogger<DropoutRiskBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);

            // Run weekly on Sunday at 7AM IST
            if (nowIst.DayOfWeek == DayOfWeek.Sunday && nowIst.Hour == 7 && nowIst.Minute < 5)
            {
                try
                {
                    _logger.LogInformation("DropoutRiskBackgroundService: recalculating risk scores");
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IDropoutRiskService>();
                    await service.RecalculateAllAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DropoutRiskBackgroundService failed");
                }

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
