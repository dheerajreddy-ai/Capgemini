using EduVoice.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.BackgroundServices;

public class RetryBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RetryBackgroundService> _logger;

    public RetryBackgroundService(IServiceScopeFactory scopeFactory, ILogger<RetryBackgroundService> logger)
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
                var callService = scope.ServiceProvider.GetRequiredService<ICallService>();
                await callService.ProcessScheduledRetriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RetryBackgroundService tick failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
