using EduVoice.Application.Common;
using EduVoice.Application.DTOs.FeeCollection;

namespace EduVoice.Application.Interfaces;

public interface IFeeCollectionService
{
    Task<ApiResponse<FeeCollectionDashboardDto>> GetDashboardAsync(Guid schoolId);
}
