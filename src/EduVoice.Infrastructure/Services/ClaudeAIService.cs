using System.Text.Json;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.Services;

public class ClaudeAIService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudeAIService> _logger;
    private readonly HttpClient _httpClient;

    public ClaudeAIService(IConfiguration configuration, ILogger<ClaudeAIService> logger, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("claude");
    }

    public async Task<CallAnalysisResult?> AnalyzeTranscriptAsync(string transcript, CallType callType)
    {
        try
        {
            var apiKey = _configuration["ANTHROPIC_API_KEY"];
            if (string.IsNullOrEmpty(apiKey)) return null;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var prompt = $"""
                Analyze this Telugu school phone call transcript and extract key information.
                Call type: {callType}

                Transcript:
                {transcript}

                Return only a valid JSON object with exactly these fields:
                {{
                    "sentiment": "Positive|Neutral|Negative|Angry",
                    "feesConfirmed": true or false,
                    "hasComplaint": true or false,
                    "complaintSummary": "brief complaint description or null",
                    "callbackRequested": true or false,
                    "summary": "2-3 sentence summary in English"
                }}
                """;

            var requestBody = new
            {
                model = "claude-sonnet-4-6",
                max_tokens = 1024,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.anthropic.com/v1/messages", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Claude API returned {StatusCode}", response.StatusCode);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseContent);

            var contentText = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(contentText)) return null;

            var jsonStart = contentText.IndexOf('{');
            var jsonEnd = contentText.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd < 0) return null;

            var jsonPart = contentText.Substring(jsonStart, jsonEnd - jsonStart + 1);
            var parsed = JsonSerializer.Deserialize<JsonElement>(jsonPart);

            return new CallAnalysisResult
            {
                Sentiment = Enum.TryParse<SentimentType>(
                    parsed.TryGetProperty("sentiment", out var s) ? s.GetString() : "Neutral",
                    true, out var sentiment) ? sentiment : SentimentType.Neutral,
                FeesConfirmed = parsed.TryGetProperty("feesConfirmed", out var fc) && fc.GetBoolean(),
                HasComplaint = parsed.TryGetProperty("hasComplaint", out var hc) && hc.GetBoolean(),
                ComplaintSummary = parsed.TryGetProperty("complaintSummary", out var cs) ? cs.GetString() : null,
                CallbackRequested = parsed.TryGetProperty("callbackRequested", out var cr) && cr.GetBoolean(),
                Summary = parsed.TryGetProperty("summary", out var sum) ? sum.GetString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing transcript with Claude AI");
            return null;
        }
    }

    public async Task<ComplaintCategory> CategorizeComplaintAsync(string complaintText)
    {
        try
        {
            var apiKey = _configuration["ANTHROPIC_API_KEY"];
            if (string.IsNullOrEmpty(apiKey)) return ComplaintCategory.Other;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var prompt = $"""
                Categorize this school complaint into one of these categories: Teacher, Fees, Facility, Academic, Behaviour, Other

                Complaint: {complaintText}

                Return only the category name, nothing else.
                """;

            var requestBody = new
            {
                model = "claude-sonnet-4-6",
                max_tokens = 50,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.anthropic.com/v1/messages", requestBody);
            if (!response.IsSuccessStatusCode) return ComplaintCategory.Other;

            var responseContent = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseContent);

            var categoryText = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString()?.Trim();

            return Enum.TryParse<ComplaintCategory>(categoryText, true, out var category)
                ? category
                : ComplaintCategory.Other;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error categorizing complaint with Claude AI");
            return ComplaintCategory.Other;
        }
    }
}

public class CallAnalysisResult
{
    public SentimentType Sentiment { get; set; }
    public bool FeesConfirmed { get; set; }
    public bool HasComplaint { get; set; }
    public string? ComplaintSummary { get; set; }
    public bool CallbackRequested { get; set; }
    public string? Summary { get; set; }
}
