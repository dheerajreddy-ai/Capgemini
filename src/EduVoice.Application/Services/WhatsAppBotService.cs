using System.Net.Http.Json;
using System.Text.Json;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class WhatsAppBotService : IWhatsAppBotService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppBotService> _logger;

    // Telugu keyword → intent mapping
    private static readonly Dictionary<string, BotIntent> IntentKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fees"] = BotIntent.Fees,         ["fee"] = BotIntent.Fees,
        ["ఫీజు"] = BotIntent.Fees,         ["ఫీస్"] = BotIntent.Fees,
        ["బకాయి"] = BotIntent.Fees,
        ["marks"] = BotIntent.Marks,       ["mark"] = BotIntent.Marks,
        ["result"] = BotIntent.Marks,      ["results"] = BotIntent.Marks,
        ["మార్కులు"] = BotIntent.Marks,    ["రిజల్ట్"] = BotIntent.Marks,
        ["attendance"] = BotIntent.Attendance,
        ["హాజరు"] = BotIntent.Attendance,  ["అటెండెన్స్"] = BotIntent.Attendance,
        ["complaint"] = BotIntent.Complaint, ["complaints"] = BotIntent.Complaint,
        ["ఫిర్యాదు"] = BotIntent.Complaint,
        ["help"] = BotIntent.Help,         ["సహాయం"] = BotIntent.Help,
        ["hi"] = BotIntent.Help,           ["hello"] = BotIntent.Help,
        ["హలో"] = BotIntent.Help,
        ["pay"] = BotIntent.Payment,       ["payment"] = BotIntent.Payment,
        ["చెల్లించు"] = BotIntent.Payment,
    };

    public WhatsAppBotService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<WhatsAppBotService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> HandleInboundMessageAsync(string fromPhone, string toPhone, string messageBody)
    {
        try
        {
            // Look up school by the Twilio WhatsApp number (To field)
            var toNormalized = toPhone.Replace("whatsapp:", "").Trim();
            var school = await _unitOfWork.Schools.Query()
                .Where(s => s.IsActive && (s.TwilioPhoneNumber == toNormalized || s.TwilioPhoneNumber == toPhone))
                .FirstOrDefaultAsync();

            if (school == null)
            {
                _logger.LogWarning("Inbound WhatsApp to unrecognized number {To}", toPhone);
                return "నమస్కారం! మీరు సరైన నంబర్‌కు WhatsApp చేశారు. (Hello! You've reached the right number.)";
            }

            // Look up student by parent phone
            var fromNormalized = fromPhone.Replace("whatsapp:", "").Trim();
            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == school.Id && !s.IsDeleted
                    && (s.ParentPhone == fromNormalized || s.ParentPhone == fromPhone))
                .FirstOrDefaultAsync();

            var intent = DetectIntent(messageBody.Trim());
            _logger.LogInformation("WhatsApp bot: From={From}, School={School}, Intent={Intent}", fromNormalized, school.Name, intent);

            if (student == null)
                return BuildUnknownParentReply(school.Name, intent);

            return intent switch
            {
                BotIntent.Fees       => BuildFeesReply(student, school),
                BotIntent.Marks      => BuildMarksReply(student, school),
                BotIntent.Attendance => BuildAttendanceReply(student, school),
                BotIntent.Payment    => BuildPaymentReply(student, school),
                BotIntent.Complaint  => await BuildComplaintReplyAsync(student, school),
                BotIntent.Help       => BuildHelpReply(student, school),
                _                    => await BuildAiReplyAsync(messageBody, student, school),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WhatsApp bot for {From}", fromPhone);
            return "క్షమించండి, ఒక సమస్య వచ్చింది. దయచేసి మళ్ళీ ప్రయత్నించండి. (Sorry, an error occurred. Please try again.)";
        }
    }

    private static BotIntent DetectIntent(string message)
    {
        var lower = message.ToLower().Trim();
        foreach (var (keyword, intent) in IntentKeywords)
            if (lower.Contains(keyword.ToLower())) return intent;
        return BotIntent.Unknown;
    }

    private static string BuildFeesReply(Student student, School school)
    {
        var name = $"{student.FirstName} {student.LastName}".Trim();
        var statusEmoji = student.FeesStatus switch
        {
            FeesStatus.Paid    => "✅",
            FeesStatus.Partial => "⚠️",
            FeesStatus.Unpaid  => "❌",
            FeesStatus.Overdue => "🔴",
            _ => "ℹ️"
        };

        var reply = $"""
            {statusEmoji} *{school.Name}*
            ─────────────────
            విద్యార్థి: *{name}*
            తరగతి: {student.Class ?? "N/A"}

            💰 *ఫీజు వివరాలు (Fee Details)*
            మొత్తం ఫీజు: ₹{student.TotalFees:N0}
            చెల్లించారు: ₹{student.PaidFees:N0}
            బకాయి: ₹{student.PendingFees:N0}
            స్థితి: {student.FeesStatus}
            """;

        if (student.FeesDueDate.HasValue)
            reply += $"\nచెల్లింపు తేదీ: {student.FeesDueDate.Value:dd MMM yyyy}";

        if (student.PendingFees > 0 && !string.IsNullOrEmpty(student.PaymentLink))
            reply += $"\n\n💳 *ఇప్పుడే చెల్లించండి:*\n{student.PaymentLink}";
        else if (student.PendingFees > 0)
            reply += "\n\n💳 Payment link కోసం 'pay' అని type చేయండి.";

        reply += "\n─────────────────\nమరిన్ని వివరాలకు 'help' అని type చేయండి.";
        return reply;
    }

    private static string BuildMarksReply(Student student, School school)
    {
        var name = $"{student.FirstName} {student.LastName}".Trim();

        if (student.Percentage == null)
            return $"*{school.Name}*\n\n{name} కోసం మార్కులు ఇంకా అందుబాటులో లేవు.\n(Marks not yet available for {name}.)";

        var grade = student.Grade ?? "N/A";
        var gradeEmoji = grade switch { "A+" or "A" => "🌟", "B" => "👍", "C" => "📚", _ => "📖" };

        var reply = $"""
            {gradeEmoji} *{school.Name}*
            ─────────────────
            విద్యార్థి: *{name}* | తరగతి: {student.Class ?? "N/A"}

            📊 *పరీక్ష ఫలితాలు (Exam Results)*
            """;

        var subjects = new[] {
            ("గణితం (Maths)", student.MathMarks),
            ("సైన్స్ (Science)", student.ScienceMarks),
            ("ఇంగ్లీష్ (English)", student.EnglishMarks),
            ("తెలుగు (Telugu)", student.TeluguMarks),
            ("సాంఘికం (Social)", student.SocialMarks),
        };

        foreach (var (sub, marks) in subjects)
            if (marks.HasValue) reply += $"\n{sub}: *{marks:0}*";

        reply += $"\n─────────────────\n📈 మొత్తం: *{student.Percentage:0.0}%* | గ్రేడ్: *{grade}*";
        return reply;
    }

    private static string BuildAttendanceReply(Student student, School school)
    {
        var name = $"{student.FirstName} {student.LastName}".Trim();

        if (student.AttendancePercentage == null)
            return $"*{school.Name}*\n\n{name} కోసం హాజరు సమాచారం అందుబాటులో లేదు.\n(Attendance data not available.)";

        var pct = student.AttendancePercentage.Value;
        var emoji = pct >= 90 ? "🌟" : pct >= 75 ? "✅" : pct >= 60 ? "⚠️" : "❌";
        var msg = pct >= 75 ? "మంచి హాజరు! (Good attendance!)" : "హాజరు తక్కువగా ఉంది. దయచేసి మెరుగుపరచండి. (Low attendance. Please improve.)";

        return $"""
            {emoji} *{school.Name}*
            ─────────────────
            విద్యార్థి: *{name}* | తరగతి: {student.Class ?? "N/A"}

            📅 *హాజరు వివరాలు (Attendance)*
            హాజరు శాతం: *{pct:0.0}%*
            వచ్చిన రోజులు: {student.AttendancePresentDays ?? 0}/{student.AttendanceTotalDays ?? 0}

            {msg}
            ─────────────────
            మరిన్ని వివరాలకు 'help' అని type చేయండి.
            """;
    }

    private static string BuildPaymentReply(Student student, School school)
    {
        var name = $"{student.FirstName} {student.LastName}".Trim();

        if (student.PendingFees <= 0)
            return $"✅ *{school.Name}*\n\n{name} కోసం అన్ని ఫీజులు చెల్లించారు!\n(All fees paid for {name}!)";

        if (!string.IsNullOrEmpty(student.PaymentLink))
            return $"""
                💳 *{school.Name} — Payment Link*
                ─────────────────
                విద్యార్థి: *{name}*
                బకాయి మొత్తం: *₹{student.PendingFees:N0}*

                👇 క్రింది link ద్వారా UPI లో చెల్లించండి:
                {student.PaymentLink}

                చెల్లించిన తర్వాత రసీదు పంపబడుతుంది.
                (Receipt will be sent after payment.)
                """;

        return $"💳 *{school.Name}*\n\nమీ బకాయి: ₹{student.PendingFees:N0}\n\nPayment link కోసం school ను సంప్రదించండి.\n(Contact school for payment link.)";
    }

    private async Task<string> BuildComplaintReplyAsync(Student student, School school)
    {
        // Find open complaints
        var openComplaints = await _unitOfWork.Complaints.Query()
            .Where(c => c.StudentId == student.Id && c.SchoolId == school.Id
                && c.Status != ComplaintStatus.Resolved && c.Status != ComplaintStatus.Closed)
            .CountAsync();

        var name = $"{student.FirstName} {student.LastName}".Trim();
        return $"""
            📋 *{school.Name} — ఫిర్యాదులు (Complaints)*
            ─────────────────
            విద్యార్థి: *{name}*
            తెరిచిన ఫిర్యాదులు: *{openComplaints}*

            ఫిర్యాదు నమోదు చేయడానికి school కి నేరుగా call చేయండి లేదా portal ని ఉపయోగించండి.
            (To raise a complaint, call the school directly or use the parent portal.)
            """;
    }

    private static string BuildHelpReply(Student student, School school)
    {
        var name = $"{student.FirstName} {student.LastName}".Trim();
        return $"""
            👋 *నమస్కారం! {school.Name} WhatsApp Bot కు స్వాగతం!*
            ─────────────────
            విద్యార్థి: *{name}* | తరగతి: {student.Class ?? "N/A"}

            మీకు ఏ సమాచారం కావాలో type చేయండి:

            💰 *fees* — ఫీజు బకాయి చూపించు
            📊 *marks* — పరీక్ష ఫలితాలు చూపించు
            📅 *attendance* — హాజరు శాతం చూపించు
            💳 *pay* — Payment link పంపించు
            📋 *complaint* — ఫిర్యాదు స్థితి చూపించు

            ─────────────────
            🕐 Calling hours: 11 AM – 6 PM IST
            📞 School: {school.ContactPhone}
            """;
    }

    private static string BuildUnknownParentReply(string schoolName, BotIntent intent)
    {
        return $"""
            👋 *నమస్కారం! {schoolName} WhatsApp Bot*
            ─────────────────
            మీ mobile number మా system లో లేదు.
            (Your number is not registered in our system.)

            దయచేసి school office ని సంప్రదించండి.
            (Please contact the school office to register.)

            సహాయం కోసం 'help' అని type చేయండి.
            """;
    }

    private async Task<string> BuildAiReplyAsync(string message, Student student, School school)
    {
        try
        {
            var apiKey = _configuration["ANTHROPIC_API_KEY"];
            if (string.IsNullOrEmpty(apiKey)) return BuildHelpReply(student, school);

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            http.DefaultRequestHeaders.Add("x-api-key", apiKey);
            http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var name = $"{student.FirstName} {student.LastName}".Trim();
            var prompt = $"""
                You are EduVoice, a helpful WhatsApp bot for {school.Name} school.
                A parent of student {name} (Class {student.Class}) sent this message: "{message}"

                Student info: Fees pending ₹{student.PendingFees}, Attendance {student.AttendancePercentage}%, Grade {student.Grade ?? "N/A"}

                Reply helpfully in Telugu (with English in parentheses where needed). Keep it under 200 words.
                If you can't help, suggest they type 'help' to see available commands.
                """;

            var body = new { model = "claude-haiku-4-5-20251001", max_tokens = 300, messages = new[] { new { role = "user", content = prompt } } };
            var resp = await http.PostAsJsonAsync("https://api.anthropic.com/v1/messages", body);
            if (!resp.IsSuccessStatusCode) return BuildHelpReply(student, school);

            var json = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString()
                   ?? BuildHelpReply(student, school);
        }
        catch
        {
            return BuildHelpReply(student, school);
        }
    }

    private enum BotIntent { Fees, Marks, Attendance, Payment, Complaint, Help, Unknown }
}
