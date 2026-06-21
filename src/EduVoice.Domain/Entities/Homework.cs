namespace EduVoice.Domain.Entities;

public class Homework
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? DueDate { get; set; }
    // Whether the daily WhatsApp alert has been sent for this homework
    public bool AlertSent { get; set; }
    public DateTime? AlertSentAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
}
