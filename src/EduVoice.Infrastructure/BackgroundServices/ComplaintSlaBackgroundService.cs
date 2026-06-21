using EduVoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.BackgroundServices;

public class ComplaintSlaBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ComplaintSlaBackgroundService> _logger;

    public ComplaintSlaBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ComplaintSlaBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IComplaintSlaService>();
                await service.CheckAndEscalateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ComplaintSlaBackgroundService failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
