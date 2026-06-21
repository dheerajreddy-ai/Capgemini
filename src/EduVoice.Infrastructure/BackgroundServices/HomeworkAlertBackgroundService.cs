using EduVoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.BackgroundServices;

/// <summary>
/// Runs daily at 4PM IST, sends WhatsApp messages to parents with today's homework.
/// </summary>
public class HomeworkAlertBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HomeworkAlertBackgroundService> _logger;

    private static readonly TimeZoneInfo Ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    public HomeworkAlertBackgroundService(IServiceScopeFactory scopeFactory, ILogger<HomeworkAlertBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);

            // Run once daily at 4PM IST
            if (nowIst.Hour == 16 && nowIst.Minute < 5)
            {
                try
                {
                    _logger.LogInformation("HomeworkAlertBackgroundService: sending daily homework alerts");
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IHomeworkService>();
                    await service.SendDailyAlertsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "HomeworkAlertBackgroundService tick failed");
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
