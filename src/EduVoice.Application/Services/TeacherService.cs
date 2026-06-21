using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Teacher;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class TeacherService : ITeacherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(IUnitOfWork unitOfWork, IAuditService auditService, ILogger<TeacherService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ApiResponse<TeacherDashboardDto>> GetDashboardAsync(Guid schoolId)
    {
        try
        {
            var students = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var todayAttendance = await _unitOfWork.Attendances.Query()
                .Where(a => a.SchoolId == schoolId && a.Date == today)
                .ToListAsync();

            var classSections = students
                .Where(s => s.Class != null)
                .GroupBy(s => new { s.Class, s.Section })
                .Select(g => new ClassSectionDto
                {
                    Class = g.Key.Class!,
                    Section = g.Key.Section,
                    StudentCount = g.Count()
                })
                .OrderBy(c => c.Class).ThenBy(c => c.Section)
                .ToList();

            var activeHomework = await _unitOfWork.Homeworks.Query()
                .CountAsync(h => h.SchoolId == schoolId && h.DueDate >= DateTime.UtcNow);

            return ApiResponse<TeacherDashboardDto>.Ok(new TeacherDashboardDto
            {
                TotalStudents = students.Count,
                TodayPresent = todayAttendance.Count(a => a.IsPresent),
                TodayAbsent = todayAttendance.Count(a => !a.IsPresent),
                AttendanceMarkedToday = todayAttendance.Count > 0,
                ActiveHomeworkCount = activeHomework,
                ClassSectionCount = classSections.Count,
                ClassSections = classSections
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching teacher dashboard for school {SchoolId}", schoolId);
            return ApiResponse<TeacherDashboardDto>.Fail("Failed to fetch dashboard", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<List<ClassSectionDto>>> GetClassSectionsAsync(Guid schoolId)
    {
        try
        {
            var result = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && s.Class != null)
                .GroupBy(s => new { s.Class, s.Section })
                .Select(g => new ClassSectionDto
                {
                    Class = g.Key.Class!,
                    Section = g.Key.Section,
                    StudentCount = g.Count()
                })
                .OrderBy(c => c.Class).ThenBy(c => c.Section)
                .ToListAsync();

            return ApiResponse<List<ClassSectionDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching class sections for school {SchoolId}", schoolId);
            return ApiResponse<List<ClassSectionDto>>.Fail("Failed to fetch class sections", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<List<TeacherStudentDto>>> GetStudentsAsync(Guid schoolId, string className, string? section)
    {
        try
        {
            var students = await GetStudentsInternalAsync(schoolId, className, section);

            var dtos = students.Select(s => new TeacherStudentDto
            {
                Id = s.Id,
                StudentCode = s.StudentId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Class = s.Class,
                Section = s.Section,
                AttendancePercentage = s.AttendancePercentage,
                MathMarks = s.MathMarks,
                ScienceMarks = s.ScienceMarks,
                EnglishMarks = s.EnglishMarks,
                TeluguMarks = s.TeluguMarks,
                SocialMarks = s.SocialMarks,
                Percentage = s.Percentage,
                Grade = s.Grade
            }).ToList();

            return ApiResponse<List<TeacherStudentDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching students for school {SchoolId}", schoolId);
            return ApiResponse<List<TeacherStudentDto>>.Fail("Failed to fetch students", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<AttendanceResultDto>> GetAttendanceAsync(Guid schoolId, DateTime date, string className, string? section)
    {
        try
        {
            var dateOnly = date.Date;
            var students = await GetStudentsInternalAsync(schoolId, className, section);
            var studentIds = students.Select(s => s.Id).ToList();

            var records = await _unitOfWork.Attendances.Query()
                .Where(a => a.SchoolId == schoolId && a.Date == dateOnly && studentIds.Contains(a.StudentId))
                .ToListAsync();

            var entries = students.Select(s =>
            {
                var r = records.FirstOrDefault(a => a.StudentId == s.Id);
                return new AttendanceEntryDto
                {
                    StudentId = s.Id,
                    StudentName = $"{s.FirstName} {s.LastName}".Trim(),
                    IsPresent = r?.IsPresent ?? false,
                    Remarks = r?.Remarks
                };
            }).ToList();

            return ApiResponse<AttendanceResultDto>.Ok(new AttendanceResultDto
            {
                Date = date,
                Class = className,
                Section = section,
                TotalStudents = students.Count,
                PresentCount = entries.Count(e => e.IsPresent),
                AbsentCount = entries.Count(e => !e.IsPresent),
                AlreadySaved = records.Count > 0,
                Entries = entries
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching attendance for school {SchoolId}", schoolId);
            return ApiResponse<AttendanceResultDto>.Fail("Failed to fetch attendance", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<AttendanceResultDto>> MarkAttendanceAsync(Guid schoolId, Guid teacherId, MarkAttendanceRequest request)
    {
        try
        {
            var dateOnly = request.Date.Date;
            var studentIds = request.Entries.Select(e => e.StudentId).ToList();

            var existing = await _unitOfWork.Attendances.Query()
                .Where(a => a.SchoolId == schoolId && a.Date == dateOnly && studentIds.Contains(a.StudentId))
                .ToListAsync();

            foreach (var entry in request.Entries)
            {
                var record = existing.FirstOrDefault(a => a.StudentId == entry.StudentId);
                if (record != null)
                {
                    record.IsPresent = entry.IsPresent;
                    record.Remarks = entry.Remarks;
                    record.MarkedByUserId = teacherId;
                    _unitOfWork.Attendances.Update(record);
                }
                else
                {
                    await _unitOfWork.Attendances.AddAsync(new Attendance
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = schoolId,
                        StudentId = entry.StudentId,
                        MarkedByUserId = teacherId,
                        Date = dateOnly,
                        IsPresent = entry.IsPresent,
                        Remarks = entry.Remarks
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Recompute attendance stats for each student from DB
            var students = await _unitOfWork.Students.Query()
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();

            foreach (var student in students)
            {
                var allForStudent = await _unitOfWork.Attendances.Query()
                    .Where(a => a.SchoolId == schoolId && a.StudentId == student.Id)
                    .ToListAsync();

                student.AttendanceTotalDays = allForStudent.Count;
                student.AttendancePresentDays = allForStudent.Count(a => a.IsPresent);
                student.AttendancePercentage = allForStudent.Count > 0
                    ? Math.Round((decimal)allForStudent.Count(a => a.IsPresent) / allForStudent.Count * 100, 1)
                    : 0;
                _unitOfWork.Students.Update(student);
            }

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, teacherId, "ATTENDANCE_MARKED", "Attendance",
                $"{request.Class}/{request.Section}/{request.Date:yyyy-MM-dd}");

            return await GetAttendanceAsync(schoolId, request.Date, request.Class, request.Section);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking attendance for school {SchoolId}", schoolId);
            return ApiResponse<AttendanceResultDto>.Fail("Failed to mark attendance", "MARK_ERROR");
        }
    }

    public async Task<ApiResponse> UploadMarksAsync(Guid schoolId, Guid teacherId, UploadMarksRequest request)
    {
        try
        {
            var studentIds = request.Entries.Select(e => e.StudentId).ToList();
            var students = await _unitOfWork.Students.Query()
                .Where(s => studentIds.Contains(s.Id) && s.SchoolId == schoolId)
                .ToListAsync();

            foreach (var entry in request.Entries)
            {
                var student = students.FirstOrDefault(s => s.Id == entry.StudentId);
                if (student == null) continue;

                if (entry.MathMarks.HasValue) student.MathMarks = entry.MathMarks;
                if (entry.ScienceMarks.HasValue) student.ScienceMarks = entry.ScienceMarks;
                if (entry.EnglishMarks.HasValue) student.EnglishMarks = entry.EnglishMarks;
                if (entry.TeluguMarks.HasValue) student.TeluguMarks = entry.TeluguMarks;
                if (entry.SocialMarks.HasValue) student.SocialMarks = entry.SocialMarks;

                var filledMarks = new[] { student.MathMarks, student.ScienceMarks, student.EnglishMarks, student.TeluguMarks, student.SocialMarks }
                    .Where(m => m.HasValue).Select(m => m!.Value).ToList();

                if (filledMarks.Count > 0)
                {
                    student.TotalMarks = filledMarks.Sum();
                    student.MaxMarks = request.MaxMarksPerSubject * filledMarks.Count;
                    student.Percentage = student.MaxMarks > 0
                        ? Math.Round(student.TotalMarks.Value / student.MaxMarks.Value * 100, 1)
                        : 0;
                    student.Grade = ComputeGrade(student.Percentage ?? 0);
                }

                _unitOfWork.Students.Update(student);

                await _unitOfWork.TeacherMarksList.AddAsync(new TeacherMarks
                {
                    Id = Guid.NewGuid(),
                    SchoolId = schoolId,
                    StudentId = entry.StudentId,
                    UploadedByUserId = teacherId,
                    ExamType = request.ExamType,
                    ExamDate = request.ExamDate,
                    MathMarks = entry.MathMarks,
                    ScienceMarks = entry.ScienceMarks,
                    EnglishMarks = entry.EnglishMarks,
                    TeluguMarks = entry.TeluguMarks,
                    SocialMarks = entry.SocialMarks,
                    MaxMarks = request.MaxMarksPerSubject
                });
            }

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, teacherId, "MARKS_UPLOADED", "TeacherMarks",
                $"{request.Class}/{request.Section}/{request.ExamType}/{request.ExamDate:yyyy-MM-dd}");

            return ApiResponse.Ok($"Marks saved for {students.Count} students.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading marks for school {SchoolId}", schoolId);
            return ApiResponse.Fail("Failed to upload marks", "UPLOAD_ERROR");
        }
    }

    public async Task<ApiResponse> AssignHomeworkAsync(Guid schoolId, Guid teacherId, AssignHomeworkRequest request)
    {
        try
        {
            await _unitOfWork.Homeworks.AddAsync(new Homework
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                Subject = request.Subject,
                Description = request.Description,
                Class = request.Class,
                Section = request.Section,
                AssignedDate = DateTime.UtcNow,
                DueDate = request.DueDate.ToUniversalTime(),
                AlertSent = false,
                CreatedByUserId = teacherId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, teacherId, "HOMEWORK_ASSIGNED", "Homework",
                $"{request.Class}/{request.Section}/{request.Subject}");

            return ApiResponse.Ok("Homework assigned successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning homework for school {SchoolId}", schoolId);
            return ApiResponse.Fail("Failed to assign homework", "ASSIGN_ERROR");
        }
    }

    private async Task<List<Domain.Entities.Student>> GetStudentsInternalAsync(Guid schoolId, string className, string? section)
    {
        var query = _unitOfWork.Students.Query()
            .Where(s => s.SchoolId == schoolId && s.Class == className);

        if (!string.IsNullOrEmpty(section))
            query = query.Where(s => s.Section == section);

        return await query.OrderBy(s => s.FirstName).ThenBy(s => s.LastName).ToListAsync();
    }

    private static string ComputeGrade(decimal percentage) => percentage switch
    {
        >= 90 => "A+",
        >= 80 => "A",
        >= 70 => "B+",
        >= 60 => "B",
        >= 50 => "C",
        >= 40 => "D",
        _ => "F"
    };
}
