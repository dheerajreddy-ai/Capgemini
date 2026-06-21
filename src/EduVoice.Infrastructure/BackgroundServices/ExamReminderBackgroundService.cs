using EduVoice.Application.Common;
using EduVoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.BackgroundServices;

/// <summary>
/// Runs daily at 9AM IST, triggers voice calls to parents 3 days and 1 day before each exam.
/// </summary>
public class ExamReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExamReminderBackgroundService> _logger;

    private static readonly TimeZoneInfo Ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    public ExamReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ExamReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);

            // Run once daily at 9AM IST
            if (nowIst.Hour == 9 && nowIst.Minute < 5)
            {
                try
                {
                    _logger.LogInformation("ExamReminderBackgroundService: processing reminders");
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IExamScheduleService>();
                    await service.ProcessRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ExamReminderBackgroundService tick failed");
                }

                // Sleep until next check (skip remaining minutes of this hour)
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
