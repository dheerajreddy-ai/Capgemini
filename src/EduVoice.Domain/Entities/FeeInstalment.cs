namespace EduVoice.Domain.Entities;

public class FeeInstalment
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public int InstalmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? OverdueReminderSentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
