using EduVoice.Application.Common;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.BackgroundServices;

public class CarrierHealthBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CarrierHealthBackgroundService> _logger;

    public CarrierHealthBackgroundService(IServiceScopeFactory scopeFactory, ILogger<CarrierHealthBackgroundService> logger)
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
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var schools = await unitOfWork.Schools.Query().Where(s => s.IsActive).ToListAsync(stoppingToken);
                var since = DateTime.UtcNow.AddDays(-7);

                foreach (var school in schools)
                {
                    var calls = await unitOfWork.Calls.Query()
                        .Where(c => c.SchoolId == school.Id && c.Direction == CallDirection.Outbound && c.CreatedAt >= since)
                        .ToListAsync(stoppingToken);

                    if (calls.Count < 10) continue;

                    var completed = calls.Count(c => c.Status == CallStatus.Completed);
                    var prevHealth = school.CarrierHealth;
                    var newHealth = CarrierHealthHelper.Classify(calls.Count, completed);

                    school.CarrierHealth = newHealth;
                    school.CarrierHealthCheckedAt = DateTime.UtcNow;
                    unitOfWork.Schools.Update(school);

                    if (newHealth != prevHealth && newHealth != CarrierHealth.Healthy)
                    {
                        var rate = (double)completed / calls.Count;
                        var msg = CarrierHealthHelper.AlertMessage(school.Name, school.TwilioPhoneNumber ?? "N/A", newHealth, rate);
                        try { await emailService.SendEmailAsync(school.ContactEmail, "EduVoice Carrier Health Alert", msg); }
                        catch { /* best effort */ }
                        _logger.LogWarning("Carrier health changed to {Health} for school {SchoolId}", newHealth, school.Id);
                    }
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CarrierHealthBackgroundService tick failed");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
