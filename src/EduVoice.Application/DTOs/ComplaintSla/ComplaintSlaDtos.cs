using EduVoice.Domain.Enums;

namespace EduVoice.Application.DTOs.ComplaintSla;

public class ComplaintSlaConfigDto
{
    public Guid? Id { get; set; }
    public ComplaintCategory Category { get; set; }
    public int SlaHours { get; set; }
    public Guid? EscalationContactUserId { get; set; }
    public string? EscalationContactName { get; set; }
    public string? EscalationContactEmail { get; set; }
    public bool IsCustomised { get; set; }
}

public class UpsertSlaConfigRequest
{
    public ComplaintCategory Category { get; set; }
    public int SlaHours { get; set; }
    public Guid? EscalationContactUserId { get; set; }
}
