namespace EduVoice.Domain.Entities;

public class Attendance
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public Guid MarkedByUserId { get; set; }
    public DateTime Date { get; set; }   // stored as midnight UTC (date only)
    public bool IsPresent { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public User MarkedByUser { get; set; } = null!;
}
