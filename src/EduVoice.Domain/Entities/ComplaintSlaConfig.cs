using EduVoice.Domain.Enums;

namespace EduVoice.Domain.Entities;

public class ComplaintSlaConfig
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public ComplaintCategory Category { get; set; }
    public int SlaHours { get; set; }
    public Guid? EscalationContactUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
    public User? EscalationContact { get; set; }
}
