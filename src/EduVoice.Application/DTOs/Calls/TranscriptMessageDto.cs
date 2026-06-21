namespace EduVoice.Application.DTOs.Calls;

public class TranscriptMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double? Timestamp { get; set; }
}
