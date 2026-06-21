using EduVoice.Application.Common;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.BackgroundServices;

/// <summary>
/// Processes students with MissedCallCallbackPending = true.
/// Fires a callback voice call via Vapi, then clears the pending flag.
/// Runs every 10 minutes, only within the 11AM–6PM IST calling window.
/// </summary>
public class MissedCallBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MissedCallBackgroundService> _logger;

    public MissedCallBackgroundService(IServiceScopeFactory scopeFactory, ILogger<MissedCallBackgroundService> logger)
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
                if (CallingWindowHelper.IsWithinCallingWindow())
                    await ProcessPendingCallbacksAsync(stoppingToken);
                else
                    _logger.LogDebug("MissedCallBackgroundService: outside calling window, skipping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MissedCallBackgroundService tick failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }

    private async Task ProcessPendingCallbacksAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var vapiService = scope.ServiceProvider.GetRequiredService<IVapiService>();

        var pendingStudents = (await uow.Students.FindAsync(
            s => s.MissedCallCallbackPending && !s.IsDeleted && s.IsActive))
            .ToList();

        if (pendingStudents.Count == 0) return;

        _logger.LogInformation("MissedCallBackgroundService: {Count} pending callbacks", pendingStudents.Count);

        foreach (var student in pendingStudents)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                var school = await uow.Schools.GetByIdAsync(student.SchoolId);
                if (school is null) continue;

                await vapiService.InitiateOutboundCallAsync(student, school, CallType.FeeReminder, null);

                student.MissedCallCallbackPending = false;
                student.MissedCallReceivedAt = null;
                student.UpdatedAt = DateTime.UtcNow;
                await uow.Students.UpdateAsync(student);
                await uow.SaveChangesAsync();

                _logger.LogInformation("Callback initiated for student {Id} ({Name})", student.Id, student.FullName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Callback failed for student {Id}", student.Id);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
