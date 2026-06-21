using System.Text.Json;
using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Calls;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class CallService : ICallService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVapiService _vapiService;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CallService> _logger;

    public CallService(IUnitOfWork unitOfWork, IVapiService vapiService,
        IAuditService auditService, IConfiguration configuration, ILogger<CallService> logger)
    {
        _unitOfWork = unitOfWork;
        _vapiService = vapiService;
        _auditService = auditService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<CallListDto>>> GetCallsAsync(Guid schoolId, int page, int pageSize,
        CallStatus? status, CallType? type, Guid? campaignId, DateTime? from, DateTime? to)
    {
        try
        {
            var query = _unitOfWork.Calls.Query()
                .Include(c => c.Student)
                .Where(c => c.SchoolId == schoolId);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            if (type.HasValue)
                query = query.Where(c => c.Type == type.Value);

            if (campaignId.HasValue)
                query = query.Where(c => c.CampaignId == campaignId.Value);

            if (from.HasValue)
                query = query.Where(c => c.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(c => c.CreatedAt <= to.Value);

            query = query.OrderByDescending(c => c.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var dtos = items.Select(c => new CallListDto
            {
                Id = c.Id,
                StudentName = $"{c.Student.FirstName} {c.Student.LastName}".Trim(),
                Class = c.Student.Class,
                Section = c.Student.Section,
                ToPhone = c.ToPhone,
                Type = c.Type,
                Status = c.Status,
                Direction = c.Direction,
                DurationSeconds = c.DurationSeconds,
                Sentiment = c.Sentiment,
                HasComplaint = c.HasComplaint,
                StartedAt = c.StartedAt,
                CreatedAt = c.CreatedAt
            }).ToList();

            return ApiResponse<PagedResult<CallListDto>>.Ok(new PagedResult<CallListDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching calls for school {SchoolId}", schoolId);
            return ApiResponse<PagedResult<CallListDto>>.Fail("Failed to fetch calls", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<CallDetailDto>> GetCallByIdAsync(Guid schoolId, Guid callId)
    {
        try
        {
            var call = await _unitOfWork.Calls.Query()
                .Include(c => c.Student)
                .Include(c => c.Campaign)
                .Where(c => c.SchoolId == schoolId && c.Id == callId)
                .FirstOrDefaultAsync();

            if (call == null)
                return ApiResponse<CallDetailDto>.Fail("Call not found", "NOT_FOUND");

            var transcript = new List<TranscriptMessageDto>();
            if (!string.IsNullOrEmpty(call.TranscriptJson))
            {
                try
                {
                    transcript = JsonSerializer.Deserialize<List<TranscriptMessageDto>>(call.TranscriptJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                catch { /* transcript might be malformed */ }
            }

            return ApiResponse<CallDetailDto>.Ok(new CallDetailDto
            {
                Id = call.Id,
                StudentId = call.StudentId,
                StudentName = $"{call.Student.FirstName} {call.Student.LastName}".Trim(),
                Class = call.Student.Class,
                Section = call.Student.Section,
                ParentName = call.Student.ParentName,
                CampaignId = call.CampaignId,
                CampaignName = call.Campaign?.Name,
                VapiCallId = call.VapiCallId,
                Type = call.Type,
                Direction = call.Direction,
                Status = call.Status,
                ToPhone = call.ToPhone,
                FromPhone = call.FromPhone,
                StartedAt = call.StartedAt,
                EndedAt = call.EndedAt,
                DurationSeconds = call.DurationSeconds,
                RecordingUrl = call.RecordingUrl,
                Transcript = transcript,
                TranscriptText = call.TranscriptText,
                Sentiment = call.Sentiment,
                FeesConfirmed = call.FeesConfirmed,
                HasComplaint = call.HasComplaint,
                ComplaintSummary = call.ComplaintSummary,
                CallbackRequested = call.CallbackRequested,
                AiSummary = call.AiSummary,
                ErrorMessage = call.ErrorMessage,
                RetryCount = call.RetryCount,
                CreatedAt = call.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching call {CallId}", callId);
            return ApiResponse<CallDetailDto>.Fail("Failed to fetch call", "FETCH_ERROR");
        }
    }

    public async Task ProcessVapiWebhookAsync(VapiWebhookPayload payload)
    {
        try
        {
            if (payload.Call == null) return;

            var vapiCallId = payload.Call.Id;
            var call = await _unitOfWork.Calls.Query()
                .Where(c => c.VapiCallId == vapiCallId)
                .FirstOrDefaultAsync();

            if (call == null)
            {
                _logger.LogWarning("Received webhook for unknown Vapi call ID: {VapiCallId}", vapiCallId);
                return;
            }

            switch (payload.Type?.ToLower())
            {
                case "call-started":
                    call.Status = CallStatus.InProgress;
                    call.StartedAt = payload.Call.StartedAt ?? DateTime.UtcNow;
                    break;

                case "call-ended":
                    await ProcessCallEndedAsync(call, payload);
                    break;

                case "hang":
                    call.Status = CallStatus.NoAnswer;
                    call.EndedAt = DateTime.UtcNow;
                    break;

                case "error":
                    call.Status = CallStatus.Failed;
                    call.ErrorMessage = "Vapi call error";
                    call.EndedAt = DateTime.UtcNow;
                    break;
            }

            call.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Calls.Update(call);
            await _unitOfWork.SaveChangesAsync();

            if (call.CampaignId.HasValue)
                await UpdateCampaignProgressAsync(call.CampaignId.Value, call.Status);

            await TriggerN8nWebhookAsync(call);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Vapi webhook");
        }
    }

    private async Task ProcessCallEndedAsync(Call call, VapiWebhookPayload payload)
    {
        call.Status = CallStatus.Completed;
        call.EndedAt = payload.Call?.EndedAt ?? DateTime.UtcNow;

        if (call.StartedAt.HasValue && call.EndedAt.HasValue)
            call.DurationSeconds = (int)(call.EndedAt.Value - call.StartedAt.Value).TotalSeconds;

        if (!string.IsNullOrEmpty(payload.RecordingUrl))
            call.RecordingUrl = payload.RecordingUrl;

        if (payload.Messages != null && payload.Messages.Count > 0)
        {
            var transcriptMessages = payload.Messages.Select(m => new
            {
                role = m.Role,
                content = m.Message,
                timestamp = m.Time
            }).ToList();

            call.TranscriptJson = JsonSerializer.Serialize(transcriptMessages);
            call.TranscriptText = string.Join("\n", payload.Messages.Select(m => $"{m.Role}: {m.Message}"));
        }
        else if (!string.IsNullOrEmpty(payload.Transcript))
        {
            call.TranscriptText = payload.Transcript;
        }

        if (!string.IsNullOrEmpty(call.TranscriptText))
        {
            try
            {
                var student = await _unitOfWork.Students.GetByIdAsync(call.StudentId);
                if (student != null)
                {
                    var analysisResult = await AnalyzeTranscriptWithClaudeAsync(call.TranscriptText, call.Type);
                    if (analysisResult != null)
                    {
                        call.Sentiment = analysisResult.Sentiment;
                        call.FeesConfirmed = analysisResult.FeesConfirmed;
                        call.HasComplaint = analysisResult.HasComplaint;
                        call.ComplaintSummary = analysisResult.ComplaintSummary;
                        call.CallbackRequested = analysisResult.CallbackRequested;
                        call.AiSummary = analysisResult.Summary;

                        if (analysisResult.HasComplaint && !string.IsNullOrEmpty(analysisResult.ComplaintSummary))
                        {
                            await CreateComplaintFromCallAsync(call, student, analysisResult);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze transcript for call {CallId}", call.Id);
            }
        }
    }

    private async Task<CallAnalysisResult?> AnalyzeTranscriptWithClaudeAsync(string transcript, CallType callType)
    {
        try
        {
            using var httpClient = new HttpClient();
            var apiKey = _configuration["ANTHROPIC_API_KEY"];
            if (string.IsNullOrEmpty(apiKey)) return null;

            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var prompt = $"""
                Analyze this Telugu school phone call transcript and return a JSON response.
                Call type: {callType}
                Transcript: {transcript}

                Return only valid JSON with these fields:
                {{
                    "sentiment": "Positive|Neutral|Negative|Angry",
                    "feesConfirmed": true|false,
                    "hasComplaint": true|false,
                    "complaintSummary": "brief summary or null",
                    "callbackRequested": true|false,
                    "summary": "2-3 sentence summary of the call"
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

            var response = await httpClient.PostAsJsonAsync("https://api.anthropic.com/v1/messages", requestBody);
            if (!response.IsSuccessStatusCode) return null;

            var responseBody = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseBody);

            var contentText = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(contentText)) return null;

            var jsonStart = contentText.IndexOf('{');
            var jsonEnd = contentText.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd < 0) return null;

            var jsonPart = contentText.Substring(jsonStart, jsonEnd - jsonStart + 1);
            var result = JsonSerializer.Deserialize<ClaudeAnalysisResponse>(jsonPart,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null) return null;

            return new CallAnalysisResult
            {
                Sentiment = Enum.TryParse<SentimentType>(result.Sentiment, true, out var sentiment)
                    ? sentiment : SentimentType.Neutral,
                FeesConfirmed = result.FeesConfirmed,
                HasComplaint = result.HasComplaint,
                ComplaintSummary = result.ComplaintSummary,
                CallbackRequested = result.CallbackRequested,
                Summary = result.Summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Claude AI transcript analysis failed");
            return null;
        }
    }

    private async Task CreateComplaintFromCallAsync(Call call, Domain.Entities.Student student, CallAnalysisResult analysis)
    {
        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            SchoolId = call.SchoolId,
            StudentId = call.StudentId,
            CallId = call.Id,
            Category = ComplaintCategory.Other,
            Priority = ComplaintPriority.Medium,
            Status = ComplaintStatus.New,
            Summary = analysis.ComplaintSummary ?? "Complaint raised during call",
            ParentName = student.ParentName,
            ParentPhone = student.ParentPhone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Complaints.AddAsync(complaint);
    }

    private async Task UpdateCampaignProgressAsync(Guid campaignId, CallStatus callStatus)
    {
        try
        {
            var campaign = await _unitOfWork.Campaigns.Query()
                .Where(c => c.Id == campaignId)
                .FirstOrDefaultAsync();

            if (campaign == null) return;

            switch (callStatus)
            {
                case CallStatus.Completed:
                    campaign.CallsCompleted++;
                    break;
                case CallStatus.Failed:
                    campaign.CallsFailed++;
                    break;
                case CallStatus.NoAnswer:
                    campaign.CallsNoAnswer++;
                    break;
            }

            campaign.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Campaigns.Update(campaign);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update campaign progress for {CampaignId}", campaignId);
        }
    }

    private async Task TriggerN8nWebhookAsync(Call call)
    {
        try
        {
            var n8nUrl = _configuration["N8N_WEBHOOK_URL"];
            if (string.IsNullOrEmpty(n8nUrl)) return;

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var payload = new
            {
                callId = call.Id,
                studentId = call.StudentId,
                campaignId = call.CampaignId,
                status = call.Status.ToString(),
                sentiment = call.Sentiment?.ToString(),
                feesConfirmed = call.FeesConfirmed,
                hasComplaint = call.HasComplaint,
                duration = call.DurationSeconds,
                timestamp = DateTime.UtcNow
            };

            await httpClient.PostAsJsonAsync(n8nUrl, payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to trigger n8n webhook for call {CallId}", call.Id);
        }
    }

    public async Task<ApiResponse<CallDto>> RetryCallAsync(Guid schoolId, Guid callId)
    {
        try
        {
            var call = await _unitOfWork.Calls.Query()
                .Include(c => c.Student)
                .Where(c => c.SchoolId == schoolId && c.Id == callId)
                .FirstOrDefaultAsync();

            if (call == null)
                return ApiResponse<CallDto>.Fail("Call not found", "NOT_FOUND");

            if (call.Status == CallStatus.Completed || call.Status == CallStatus.InProgress)
                return ApiResponse<CallDto>.Fail("Cannot retry a completed or in-progress call", "INVALID_STATUS");

            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null)
                return ApiResponse<CallDto>.Fail("School not found", "NOT_FOUND");

            var vapiCallId = await _vapiService.InitiateOutboundCallAsync(call.Student, school, call.Type, call.CampaignId);

            var retryCall = new Call
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                StudentId = call.StudentId,
                CampaignId = call.CampaignId,
                VapiCallId = vapiCallId,
                Type = call.Type,
                Direction = CallDirection.Outbound,
                Status = CallStatus.Initiated,
                ToPhone = call.ToPhone,
                FromPhone = school.TwilioPhoneNumber,
                RetryCount = call.RetryCount + 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Calls.AddAsync(retryCall);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<CallDto>.Ok(new CallDto
            {
                Id = retryCall.Id,
                StudentId = retryCall.StudentId,
                StudentName = $"{call.Student.FirstName} {call.Student.LastName}".Trim(),
                Type = retryCall.Type,
                Direction = retryCall.Direction,
                Status = retryCall.Status,
                ToPhone = retryCall.ToPhone,
                CreatedAt = retryCall.CreatedAt
            }, "Call retry initiated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying call {CallId}", callId);
            return ApiResponse<CallDto>.Fail("Failed to retry call", "RETRY_ERROR");
        }
    }

    private class CallAnalysisResult
    {
        public SentimentType Sentiment { get; set; }
        public bool FeesConfirmed { get; set; }
        public bool HasComplaint { get; set; }
        public string? ComplaintSummary { get; set; }
        public bool CallbackRequested { get; set; }
        public string? Summary { get; set; }
    }

    private class ClaudeAnalysisResponse
    {
        public string Sentiment { get; set; } = "Neutral";
        public bool FeesConfirmed { get; set; }
        public bool HasComplaint { get; set; }
        public string? ComplaintSummary { get; set; }
        public bool CallbackRequested { get; set; }
        public string? Summary { get; set; }
    }
}
