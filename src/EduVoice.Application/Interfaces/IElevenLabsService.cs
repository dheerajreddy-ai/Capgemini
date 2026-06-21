namespace EduVoice.Application.Interfaces;

public interface IElevenLabsService
{
    Task<List<VoiceDto>> GetAvailableVoicesAsync();
    Task<string> TestVoiceAsync(string voiceId, string text);
}

public class VoiceDto
{
    public string VoiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? PreviewUrl { get; set; }
}
