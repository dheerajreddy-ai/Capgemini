namespace EduVoice.Domain.Entities;

public class PtmSchedule
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime PtmDate { get; set; }
    public string? Class { get; set; }
    public string? Section { get; set; }
    public string? Notes { get; set; }
    public bool Reminder3DaySent { get; set; }
    public bool Reminder1DaySent { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
}
