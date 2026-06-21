namespace EduVoice.Application.DTOs.Students;

public class ImportStudentsResult
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<ImportRowError> Errors { get; set; } = new();
}

public class ImportRowError
{
    public int Row { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
