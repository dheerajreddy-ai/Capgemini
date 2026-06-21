using System.Text.Json;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduVoice.Infrastructure.Services;

public class VapiService : IVapiService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<VapiService> _logger;
    private readonly HttpClient _httpClient;

    public VapiService(IConfiguration configuration, ILogger<VapiService> logger, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("vapi");
    }

    public async Task<string> InitiateOutboundCallAsync(Student student, School school, CallType callType, Guid? campaignId)
    {
        var apiKey = _configuration["VAPI_API_KEY"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("VAPI_API_KEY is not configured");

        var assistantId = school.VapiAssistantId;
        var systemPrompt = callType switch
        {
            CallType.FeeReminder => GetFeeReminderSystemPrompt(student, school),
            CallType.ProgressUpdate => GetProgressUpdateSystemPrompt(student, school),
            _ => GetFeeReminderSystemPrompt(student, school)
        };

        var requestBody = new
        {
            assistantId = !string.IsNullOrEmpty(assistantId) ? assistantId : null,
            assistant = string.IsNullOrEmpty(assistantId) ? new
            {
                model = new
                {
                    provider = "anthropic",
                    model = "claude-sonnet-4-6",
                    systemPrompt
                },
                voice = new
                {
                    provider = "elevenlabs",
                    voiceId = school.ElevenLabsVoiceId ?? "pNInz6obpgDQGcFmaJgB"
                },
                firstMessage = GetFirstMessage(student, callType),
                recordingEnabled = true,
                transcriptPlan = new { enabled = true }
            } : null,
            phoneNumberId = school.TwilioPhoneNumber,
            customer = new
            {
                number = student.ParentPhone,
                name = student.ParentName
            },
            metadata = new Dictionary<string, string>
            {
                ["schoolId"] = school.Id.ToString(),
                ["studentId"] = student.Id.ToString(),
                ["callType"] = callType.ToString(),
                ["campaignId"] = campaignId?.ToString() ?? string.Empty
            }
        };

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await _httpClient.PostAsJsonAsync("https://api.vapi.ai/call/phone", requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Vapi API error: {StatusCode} - {Error}", response.StatusCode, error);
            throw new Exception($"Vapi call failed: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(responseContent);

        if (!doc.RootElement.TryGetProperty("id", out var idElement))
            throw new Exception("Vapi response missing call ID");

        return idElement.GetString() ?? throw new Exception("Vapi call ID is empty");
    }

    public string GetFeeReminderSystemPrompt(Student student, School school)
    {
        var pendingAmount = student.PendingFees;
        var dueDate = student.FeesDueDate?.ToString("dd MMMM yyyy") ?? "వీలైనంత త్వరగా";

        return $"""
            మీరు {school.Name} పాఠశాల తరఫున ఫీజు రిమైండర్ కోసం కాల్ చేస్తున్నారు.
            మీరు Telugu లో మాట్లాడాలి.

            విద్యార్థి పేరు: {student.FirstName} {student.LastName}
            తరగతి: {student.Class}{student.Section}
            పెండింగ్ ఫీజు: ₹{pendingAmount:N0}
            చెల్లించాల్సిన తేదీ: {dueDate}

            మీరు చేయవలసినవి:
            1. మర్యాదగా మాట్లాడండి
            2. ఫీజు సమయానికి చెల్లించమని అడగండి
            3. పేరెంట్ ఏదైనా ఇబ్బంది చెప్తే వినండి
            4. వారికి సహాయం అవసరమైతే నోట్ చేయండి
            5. కాల్ ముగిసే ముందు ఫీజు నిర్ధారించినారా అని అడగండి

            ముఖ్యమైన విషయాలు:
            - ఫీజు చెల్లిస్తారని నిర్ధారించుకోండి (FeesConfirmed: true/false)
            - ఏదైనా ఫిర్యాదు ఉంటే నోట్ చేయండి (HasComplaint: true/false)
            - తిరిగి కాల్ అవసరమైతే గుర్తుపెట్టుకోండి (CallbackRequested: true/false)
            """;
    }

    public string GetProgressUpdateSystemPrompt(Student student, School school)
    {
        var percentage = student.Percentage?.ToString("F1") ?? "అందుబాటులో లేదు";
        var grade = student.Grade ?? "N/A";
        var attendance = student.AttendancePercentage?.ToString("F1") ?? "అందుబాటులో లేదు";

        return $"""
            మీరు {school.Name} పాఠశాల తరఫున విద్యార్థి పురోగతి అప్డేట్ కోసం కాల్ చేస్తున్నారు.
            మీరు Telugu లో మాట్లాడాలి.

            విద్యార్థి పేరు: {student.FirstName} {student.LastName}
            తరగతి: {student.Class}{student.Section}
            మొత్తం పర్సెంటేజ్: {percentage}%
            గ్రేడ్: {grade}
            హాజరు: {attendance}%

            సబ్జెక్ట్ మార్కులు:
            - గణితం: {student.MathMarks?.ToString() ?? "N/A"}
            - సైన్స్: {student.ScienceMarks?.ToString() ?? "N/A"}
            - ఇంగ్లీష్: {student.EnglishMarks?.ToString() ?? "N/A"}
            - తెలుగు: {student.TeluguMarks?.ToString() ?? "N/A"}
            - సామాజికం: {student.SocialMarks?.ToString() ?? "N/A"}

            మీరు చేయవలసినవి:
            1. మర్యాదగా మాట్లాడండి
            2. విద్యార్థి పురోగతి గురించి సమాచారం ఇవ్వండి
            3. మెరుగుపడవలసిన విషయాలు చెప్పండి
            4. పేరెంట్ ఏదైనా అడిగితే సహాయం చేయండి
            5. ఏదైనా ఫిర్యాదు ఉంటే నోట్ చేయండి
            """;
    }

    private static string GetFirstMessage(Student student, CallType callType)
    {
        return callType switch
        {
            CallType.FeeReminder =>
                $"నమస్కారం, నేను {student.FirstName} యొక్క పాఠశాల నుండి మాట్లాడుతున్నాను. {student.ParentName} గారు మాట్లాడుతున్నారా?",
            CallType.ProgressUpdate =>
                $"నమస్కారం, నేను {student.FirstName} యొక్క పాఠశాల నుండి పురోగతి అప్డేట్ కోసం కాల్ చేస్తున్నాను. {student.ParentName} గారు మాట్లాడుతున్నారా?",
            _ =>
                $"నమస్కారం, నేను పాఠశాల నుండి మాట్లాడుతున్నాను. {student.ParentName} గారు మాట్లాడుతున్నారా?"
        };
    }
}
