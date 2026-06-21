namespace EduVoice.Application.DTOs.Homework;

public class HomeworkDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public bool AlertSent { get; set; }
    public DateTime? AlertSentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateHomeworkRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Class { get; set; }
    public string? Section { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? DueDate { get; set; }
}
