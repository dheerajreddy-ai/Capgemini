using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Teacher;

namespace EduVoice.Application.Interfaces;

public interface ITeacherService
{
    Task<ApiResponse<TeacherDashboardDto>> GetDashboardAsync(Guid schoolId);
    Task<ApiResponse<List<ClassSectionDto>>> GetClassSectionsAsync(Guid schoolId);
    Task<ApiResponse<List<TeacherStudentDto>>> GetStudentsAsync(Guid schoolId, string className, string? section);
    Task<ApiResponse<AttendanceResultDto>> GetAttendanceAsync(Guid schoolId, DateTime date, string className, string? section);
    Task<ApiResponse<AttendanceResultDto>> MarkAttendanceAsync(Guid schoolId, Guid teacherId, MarkAttendanceRequest request);
    Task<ApiResponse> UploadMarksAsync(Guid schoolId, Guid teacherId, UploadMarksRequest request);
    Task<ApiResponse> AssignHomeworkAsync(Guid schoolId, Guid teacherId, AssignHomeworkRequest request);
}
