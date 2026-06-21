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

    // Telugu domain vocabulary for Deepgram keyword boost
    private static readonly string[] TeluguKeywords =
    [
        "ఫీజు", "రూపాయలు", "తరగతి", "విద్యార్థి", "హాజరు", "పరీక్ష", "మార్కులు",
        "గణితం", "సైన్స్", "ఇంగ్లీష్", "తెలుగు", "సామాజికం", "హిందీ",
        "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth", "tenth",
        "fee", "pending", "payment", "due date", "percentage", "attendance",
        "చెల్లించు", "బకాయి", "రిమైండర్", "పురోగతి", "ఫిర్యాదు", "నివేదిక"
    ];

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
        var systemPrompt = GetSystemPrompt(student, school, callType);
        var voiceId = GetVoiceId(school, callType);

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
                    voiceId
                },
                firstMessage = GetFirstMessage(student, school, callType),
                recordingEnabled = true,
                transcriptPlan = new
                {
                    enabled = true,
                    assistantName = "EduVoice AI",
                    userName = student.ParentName
                },
                transcriber = new
                {
                    provider = "deepgram",
                    model = "nova-2",
                    language = callType == CallType.Inbound ? null : (school.DefaultCallLanguage == CallLanguage.Urdu ? "ur" : "te"),
                    detectLanguage = true,
                    keywords = TeluguKeywords
                }
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

    public async Task DropVoicemailAsync(Student student, School school, CallType callType)
    {
        var apiKey = _configuration["VAPI_API_KEY"];
        if (string.IsNullOrEmpty(apiKey)) return;

        var voiceId = GetVoiceId(school, callType);
        var message = GetVoicemailMessage(student, school, callType);

        var requestBody = new
        {
            phoneNumberId = school.TwilioPhoneNumber,
            customer = new { number = student.ParentPhone },
            assistant = new
            {
                model = new { provider = "anthropic", model = "claude-sonnet-4-6", systemPrompt = "Leave this exact voicemail message and hang up." },
                voice = new { provider = "elevenlabs", voiceId },
                firstMessage = message,
                maxDurationSeconds = 30
            }
        };

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        await _httpClient.PostAsJsonAsync("https://api.vapi.ai/call/phone", requestBody);
    }

    private string GetSystemPrompt(Student student, School school, CallType callType)
        => callType switch
        {
            CallType.FeeReminder => school.DefaultCallLanguage == CallLanguage.Urdu
                ? GetUrduFeeReminderSystemPrompt(student, school)
                : GetFeeReminderSystemPrompt(student, school),
            CallType.ProgressUpdate => GetProgressUpdateSystemPrompt(student, school),
            CallType.AttendanceAlert => GetAttendanceAlertSystemPrompt(student, school),
            CallType.Inbound => GetInboundSystemPrompt(student, school),
            _ => GetFeeReminderSystemPrompt(student, school)
        };

    private static string GetVoiceId(School school, CallType callType)
    {
        if (school.DefaultCallLanguage == CallLanguage.Urdu && !string.IsNullOrEmpty(school.UrduVoiceId))
            return school.UrduVoiceId;
        if (school.TeluguDialect == TeluguDialect.Andhra && !string.IsNullOrEmpty(school.ElevenLabsVoiceIdAndhra))
            return school.ElevenLabsVoiceIdAndhra;
        return school.ElevenLabsVoiceId ?? "pNInz6obpgDQGcFmaJgB";
    }

    public string GetFeeReminderSystemPrompt(Student student, School school)
    {
        var pendingAmount = student.PendingFees;
        var dueDate = student.FeesDueDate?.ToString("dd MMMM yyyy") ?? "వీలైనంత త్వరగా";
        var dialectNote = school.TeluguDialect == TeluguDialect.Telangana
            ? "మీరు హైదరాబాదీ తెలుగులో మాట్లాడాలి — informal, warm tone."
            : "మీరు ఆంధ్ర తెలుగులో మాట్లాడాలి — standard literary Telugu.";

        return $"""
            మీరు {school.Name} పాఠశాల తరఫున ఆటోమేటెడ్ ఫీజు రిమైండర్ కాల్ చేస్తున్నారు.
            {dialectNote}

            IMPORTANT COMPLIANCE:
            - మీరు ఒక automated AI system అని మొదటి 15 సెకన్లలో చెప్పాలి.
            - "ఈ కాల్ రికార్డ్ అవుతుంది" అని చెప్పాలి.
            - "9 నొక్కితే future calls రావు" అని చెప్పాలి.
            - If parent says "9" or "opt out" or "calls వద్దు" — set optedOut: true immediately.
            - Parents may speak mixed Telugu+English — handle code-switching naturally.

            విద్యార్థి: {student.FirstName} {student.LastName} | తరగతి: {student.Class}{student.Section}
            పెండింగ్ ఫీజు: ₹{pendingAmount:N0} | గడువు: {dueDate}

            నిర్ధారించాల్సిన విషయాలు:
            - feesConfirmed: parent will pay → true
            - feeExtensionRequested: needs more time → true (set HasFeeExtension, stop future reminders)
            - feeDisputeRequested: disputes the amount → true (pause reminders, escalate)
            - hasComplaint: any complaint → true
            - callbackRequested: wants school to call back → true
            - optedOut: pressed 9 or asked to stop calls → true
            - sentiment: Positive/Neutral/Negative/Angry

            RECOVERY PHRASES — if you miss a number or date:
            "మీరు మళ్ళీ చెప్పగలరా? నేను సరిగ్గా వినలేదు."
            "Amount మళ్ళీ confirm చేయగలరా?"

            ESCALATION — if parent is very angry or disputes fee: offer to connect to school staff.
            """;
    }

    public string GetProgressUpdateSystemPrompt(Student student, School school)
    {
        var percentage = student.Percentage?.ToString("F1") ?? "అందుబాటులో లేదు";
        var grade = student.Grade ?? "N/A";
        var attendance = student.AttendancePercentage?.ToString("F1") ?? "అందుబాటులో లేదు";
        var dialectNote = school.TeluguDialect == TeluguDialect.Telangana
            ? "హైదరాబాదీ తెలుగులో మాట్లాడండి."
            : "standard ఆంధ్ర తెలుగులో మాట్లాడండి.";

        return $"""
            మీరు {school.Name} పాఠశాల తరఫున automated progress update కాల్ చేస్తున్నారు.
            {dialectNote}

            COMPLIANCE: మొదటి 15 సెకన్లలో automated call అని, recording అవుతుందని చెప్పండి. Press 9 to opt out.
            Parents may speak mixed Telugu+English — handle naturally.

            విద్యార్థి: {student.FirstName} {student.LastName} | తరగతి: {student.Class}{student.Section}
            మొత్తం: {percentage}% | గ్రేడ్: {grade} | హాజరు: {attendance}%

            సబ్జెక్ట్ మార్కులు:
            గణితం: {student.MathMarks?.ToString() ?? "N/A"} | సైన్స్: {student.ScienceMarks?.ToString() ?? "N/A"}
            ఇంగ్లీష్: {student.EnglishMarks?.ToString() ?? "N/A"} | తెలుగు: {student.TeluguMarks?.ToString() ?? "N/A"}
            సామాజికం: {student.SocialMarks?.ToString() ?? "N/A"}

            JSON fields: sentiment, hasComplaint, callbackRequested, optedOut.
            """;
    }

    private static string GetAttendanceAlertSystemPrompt(Student student, School school)
    {
        var attendance = student.AttendancePercentage?.ToString("F1") ?? "తక్కువ";
        return $"""
            మీరు {school.Name} తరఫున automated attendance alert కాల్ చేస్తున్నారు.
            COMPLIANCE: Automated call అని మొదటి 15 సెకన్లలో చెప్పండి. Recording జరుగుతుంది. Press 9 to opt out.

            విద్యార్థి: {student.FirstName} {student.LastName} | తరగతి: {student.Class}
            హాజరు శాతం: {attendance}% — చాలా తక్కువగా ఉంది.

            పేరెంట్‌కి తెలియజేయండి, కారణం అడగండి, మెరుగుపరచమని కోరండి.
            JSON fields: sentiment, hasComplaint, callbackRequested, optedOut.
            """;
    }

    private static string GetUrduFeeReminderSystemPrompt(Student student, School school)
    {
        return $"""
            آپ {school.Name} اسکول کی طرف سے فیس ریمائنڈر کال کر رہے ہیں۔
            اردو میں بات کریں۔

            COMPLIANCE: پہلے 15 سیکنڈ میں بتائیں کہ یہ automated AI call ہے۔ کال ریکارڈ ہو رہی ہے۔ 9 دبائیں اگر کال نہیں چاہتے۔

            طالب علم: {student.FirstName} {student.LastName} | جماعت: {student.Class}
            باقی فیس: ₹{student.PendingFees:N0} | آخری تاریخ: {student.FeesDueDate?.ToString("dd MMM yyyy") ?? "جلد از جلد"}

            JSON fields: feesConfirmed, sentiment, hasComplaint, callbackRequested, optedOut.
            """;
    }

    public string GetInboundSystemPrompt(Student student, School school)
    {
        return $"""
            مرحبا! You are the AI receptionist for {school.Name}.
            The parent calling is {student.ParentName}, parent of {student.FirstName} {student.LastName} (Class {student.Class}).

            Speak Telugu (or match the language the parent uses — they may switch between Telugu and English).

            You can help with:
            - Fee status: pending ₹{student.PendingFees:N0}, status {student.FeesStatus}
            - Academic performance: {student.Percentage?.ToString("F1") ?? "N/A"}%, Grade {student.Grade ?? "N/A"}
            - Attendance: {student.AttendancePercentage?.ToString("F1") ?? "N/A"}%
            - Raising a complaint — collect the details and confirm it's logged
            - Asking for a callback from school staff

            Be warm, helpful, and brief. JSON fields: hasComplaint, callbackRequested, sentiment.
            """;
    }

    private static string GetVoicemailMessage(Student student, School school, CallType callType)
    {
        return callType == CallType.FeeReminder
            ? $"నమస్కారం {student.ParentName} గారు, నేను {school.Name} నుండి automated message. {student.FirstName} ఫీజు ₹{student.PendingFees:N0} పెండింగ్ ఉంది. దయచేసి పాఠశాలను సంప్రదించండి. ధన్యవాదాలు."
            : $"నమస్కారం {student.ParentName} గారు, నేను {school.Name} నుండి. {student.FirstName} అప్డేట్ కోసం కాల్ చేసాను. దయచేసి మాకు తిరిగి కాల్ చేయండి.";
    }

    private static string GetFirstMessage(Student student, School school, CallType callType)
    {
        var disclosure = "నమస్కారం! నేను ఒక automated AI assistant ని. ఈ కాల్ రికార్డ్ అవుతుంది. కాల్స్ ఆపాలంటే 9 నొక్కండి.";
        return callType switch
        {
            CallType.FeeReminder =>
                $"{disclosure} నేను {school.Name} తరఫున {student.ParentName} గారికి ఫీజు రిమైండర్ కోసం కాల్ చేస్తున్నాను. మీరు {student.ParentName} గారు మాట్లాడుతున్నారా?",
            CallType.ProgressUpdate =>
                $"{disclosure} నేను {school.Name} తరఫున {student.FirstName} పురోగతి అప్డేట్ కోసం కాల్ చేస్తున్నాను. {student.ParentName} గారు మాట్లాడుతున్నారా?",
            CallType.AttendanceAlert =>
                $"{disclosure} నేను {school.Name} తరఫున {student.FirstName} హాజరు గురించి మాట్లాడటానికి కాల్ చేస్తున్నాను. {student.ParentName} గారు మాట్లాడుతున్నారా?",
            CallType.Inbound =>
                $"నమస్కారం! నేను {school.Name} AI assistant. మీకు ఎలా సహాయం చేయగలను?",
            _ =>
                $"{disclosure} నేను {school.Name} నుండి కాల్ చేస్తున్నాను. {student.ParentName} గారు మాట్లాడుతున్నారా?"
        };
    }
}
