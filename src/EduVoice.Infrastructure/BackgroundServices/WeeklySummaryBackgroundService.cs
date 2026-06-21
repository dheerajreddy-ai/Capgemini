using EduVoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.BackgroundServices;

public class WeeklySummaryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeeklySummaryBackgroundService> _logger;

    private static readonly TimeZoneInfo Ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    public WeeklySummaryBackgroundService(IServiceScopeFactory scopeFactory,
        ILogger<WeeklySummaryBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);

            // Monday evenings at 7PM IST
            if (nowIst.DayOfWeek == DayOfWeek.Monday && nowIst.Hour == 19 && nowIst.Minute < 5)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IParentEngagementService>();
                    await service.SendWeeklySummariesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WeeklySummaryBackgroundService failed");
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
