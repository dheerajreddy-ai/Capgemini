using System.Text;
using System.Text.Json;
using EduVoice.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.Services;

public class ElevenLabsService : IElevenLabsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ElevenLabsService> _logger;
    private readonly HttpClient _httpClient;

    public ElevenLabsService(IConfiguration configuration, ILogger<ElevenLabsService> logger, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("elevenlabs");
    }

    public async Task<List<VoiceDto>> GetAvailableVoicesAsync()
    {
        try
        {
            var apiKey = _configuration["ELEVENLABS_API_KEY"];
            if (string.IsNullOrEmpty(apiKey))
                return new List<VoiceDto>();

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("xi-api-key", apiKey);

            var response = await _httpClient.GetAsync("https://api.elevenlabs.io/v1/voices");
            if (!response.IsSuccessStatusCode)
                return new List<VoiceDto>();

            var content = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);

            var voices = new List<VoiceDto>();
            if (doc.RootElement.TryGetProperty("voices", out var voicesElement))
            {
                foreach (var voice in voicesElement.EnumerateArray())
                {
                    voices.Add(new VoiceDto
                    {
                        VoiceId = voice.GetProperty("voice_id").GetString() ?? string.Empty,
                        Name = voice.GetProperty("name").GetString() ?? string.Empty,
                        Category = voice.TryGetProperty("category", out var cat) ? cat.GetString() : null,
                        PreviewUrl = voice.TryGetProperty("preview_url", out var prev) ? prev.GetString() : null
                    });
                }
            }

            return voices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching ElevenLabs voices");
            return new List<VoiceDto>();
        }
    }

    public async Task<string> TestVoiceAsync(string voiceId, string text)
    {
        try
        {
            var apiKey = _configuration["ELEVENLABS_API_KEY"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("ELEVENLABS_API_KEY is not configured");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("xi-api-key", apiKey);

            var requestBody = JsonSerializer.Serialize(new
            {
                text,
                model_id = "eleven_multilingual_v2",
                voice_settings = new
                {
                    stability = 0.5,
                    similarity_boost = 0.75
                }
            });

            var response = await _httpClient.PostAsync(
                $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}",
                new StringContent(requestBody, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception($"ElevenLabs API error: {response.StatusCode}");

            var audioBytes = await response.Content.ReadAsByteArrayAsync();
            return Convert.ToBase64String(audioBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing ElevenLabs voice {VoiceId}", voiceId);
            throw;
        }
    }
}
