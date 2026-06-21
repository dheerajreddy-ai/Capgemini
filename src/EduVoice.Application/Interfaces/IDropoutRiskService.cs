using EduVoice.Application.Common;
using EduVoice.Application.DTOs.DropoutRisk;

namespace EduVoice.Application.Interfaces;

public interface IDropoutRiskService
{
    Task<ApiResponse<DropoutRiskSummaryDto>> GetRiskSummaryAsync(Guid schoolId, string? riskLevel = null);
    Task RecalculateAllAsync();
}
