using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Reports;

namespace EduVoice.Application.Interfaces;

public interface IReportService
{
    Task<ApiResponse<StudentReportCardDto>> GetStudentReportCardAsync(Guid schoolId, Guid studentId);
    Task<ApiResponse<AcademicReportDto>> GetAcademicReportAsync(Guid schoolId, string? filterClass = null);
}
