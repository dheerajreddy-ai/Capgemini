using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Complaints;
using EduVoice.Domain.Enums;

namespace EduVoice.Application.Interfaces;

public interface IComplaintService
{
    Task<ApiResponse<PagedResult<ComplaintDto>>> GetComplaintsAsync(Guid schoolId, int page, int pageSize,
        ComplaintStatus? status, ComplaintCategory? category, ComplaintPriority? priority);
    Task<ApiResponse<ComplaintDto>> GetComplaintByIdAsync(Guid schoolId, Guid complaintId);
    Task<ApiResponse<ComplaintDto>> UpdateComplaintAsync(Guid schoolId, Guid complaintId, UpdateComplaintRequest request);
    Task<byte[]> ExportToExcelAsync(Guid schoolId);
}
