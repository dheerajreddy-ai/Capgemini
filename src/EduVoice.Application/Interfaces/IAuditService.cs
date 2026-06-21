namespace EduVoice.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid? schoolId, Guid? userId, string action, string entityType,
        string? entityId = null, string? oldValues = null, string? newValues = null,
        string? ipAddress = null, string? userAgent = null);
}
