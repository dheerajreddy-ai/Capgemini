using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Students;
using EduVoice.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EduVoice.Application.Interfaces;

public interface IStudentService
{
    Task<ApiResponse<PagedResult<StudentListDto>>> GetStudentsAsync(Guid schoolId, int page, int pageSize,
        string? search, string? classFilter, string? sectionFilter, FeesStatus? feesStatus, string? sortBy);
    Task<ApiResponse<StudentDto>> GetStudentByIdAsync(Guid schoolId, Guid studentId);
    Task<ApiResponse<StudentDto>> CreateStudentAsync(Guid schoolId, CreateStudentRequest request);
    Task<ApiResponse<StudentDto>> UpdateStudentAsync(Guid schoolId, Guid studentId, UpdateStudentRequest request);
    Task<ApiResponse> DeleteStudentAsync(Guid schoolId, Guid studentId);
    Task<ApiResponse<ImportStudentsResult>> ImportFromExcelAsync(Guid schoolId, IFormFile file);
    Task<byte[]> ExportToExcelAsync(Guid schoolId);
}
