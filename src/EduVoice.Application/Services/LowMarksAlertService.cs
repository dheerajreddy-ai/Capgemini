using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class LowMarksAlertService : ILowMarksAlertService
{
    private readonly IVapiService _vapiService;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LowMarksAlertService> _logger;

    public LowMarksAlertService(IVapiService vapiService, IUnitOfWork uow, ILogger<LowMarksAlertService> logger)
    {
        _vapiService = vapiService;
        _uow = uow;
        _logger = logger;
    }

    public async Task CheckAndAlertAsync(Student student, School school)
    {
        // Don't re-alert within 7 days for the same student
        if (student.LowMarksAlertSentAt.HasValue &&
            (DateTime.UtcNow - student.LowMarksAlertSentAt.Value).TotalDays < 7)
            return;

        if (student.DoNotCall) return;

        var threshold = school.LowMarksThreshold;

        var subjectMarks = new Dictionary<string, decimal?>
        {
            ["Maths"]   = student.MathMarks,
            ["Science"] = student.ScienceMarks,
            ["English"] = student.EnglishMarks,
            ["Telugu"]  = student.TeluguMarks,
            ["Social"]  = student.SocialMarks,
        };

        var lowSubjects = subjectMarks
            .Where(kv => kv.Value.HasValue && kv.Value < threshold)
            .Select(kv => kv.Key)
            .ToList();

        if (lowSubjects.Count == 0) return;

        _logger.LogInformation("Low marks alert for student {Id} ({Name}): {Subjects} below {Threshold}%",
            student.Id, $"{student.FirstName} {student.LastName}", string.Join(", ", lowSubjects), threshold);

        try
        {
            await _vapiService.InitiateOutboundCallAsync(student, school, CallType.LowMarksAlert, null);

            student.LowMarksAlertSentAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;
            _uow.Students.Update(student);
            await _uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Low marks alert call failed for student {Id}", student.Id);
        }
    }
}
