using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;

namespace EduVoice.Application.Interfaces;

public interface IVapiService
{
    Task<string> InitiateOutboundCallAsync(Student student, School school, CallType callType, Guid? campaignId);
    string GetFeeReminderSystemPrompt(Student student, School school);
    string GetProgressUpdateSystemPrompt(Student student, School school);
}
