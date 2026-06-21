using EduVoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.BackgroundServices;

public class DailySummaryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailySummaryBackgroundService> _logger;

    private static readonly TimeZoneInfo Ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    public DailySummaryBackgroundService(IServiceScopeFactory scopeFactory,
        ILogger<DailySummaryBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);

            // Fire every day at 8 AM IST; the 5-minute window avoids double-firing
            if (nowIst.Hour == 8 && nowIst.Minute < 5)
            {
                try
                {
                    _logger.LogInformation("DailySummaryBackgroundService: sending principal digests");
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IDailySummaryService>();
                    await service.SendAllSummariesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DailySummaryBackgroundService failed");
                }

                // Sleep past the 5-minute window so we don't fire again today
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
