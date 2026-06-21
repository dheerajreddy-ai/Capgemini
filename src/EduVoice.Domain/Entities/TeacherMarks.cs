using EduVoice.Domain.Enums;

namespace EduVoice.Domain.Entities;

public class TeacherMarks
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public Guid UploadedByUserId { get; set; }
    public ExamType ExamType { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal? MathMarks { get; set; }
    public decimal? ScienceMarks { get; set; }
    public decimal? EnglishMarks { get; set; }
    public decimal? TeluguMarks { get; set; }
    public decimal? SocialMarks { get; set; }
    public decimal MaxMarks { get; set; } = 100;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public School School { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public User UploadedByUser { get; set; } = null!;
}
