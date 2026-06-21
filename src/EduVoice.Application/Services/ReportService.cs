using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Reports;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IUnitOfWork uow, ILogger<ReportService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<ApiResponse<StudentReportCardDto>> GetStudentReportCardAsync(Guid schoolId, Guid studentId)
    {
        try
        {
            var student = await _uow.Students.Query()
                .Include(s => s.School)
                .FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId && !s.IsDeleted);

            if (student is null)
                return ApiResponse<StudentReportCardDto>.Fail("Student not found", "NOT_FOUND");

            var dto = new StudentReportCardDto
            {
                StudentId = student.Id,
                StudentCode = student.StudentId,
                StudentName = $"{student.FirstName} {student.LastName}",
                Class = student.Class,
                Section = student.Section,
                ParentName = student.ParentName,
                ParentPhone = student.ParentPhone,
                DateOfBirth = student.DateOfBirth,
                MathMarks = student.MathMarks,
                ScienceMarks = student.ScienceMarks,
                EnglishMarks = student.EnglishMarks,
                TeluguMarks = student.TeluguMarks,
                SocialMarks = student.SocialMarks,
                TotalMarks = student.TotalMarks,
                MaxMarks = student.MaxMarks,
                Percentage = student.Percentage,
                Grade = student.Grade,
                Remarks = student.Remarks,
                AttendancePresentDays = student.AttendancePresentDays,
                AttendanceTotalDays = student.AttendanceTotalDays,
                AttendancePercentage = student.AttendancePercentage,
                TotalFees = student.TotalFees,
                PaidFees = student.PaidFees,
                PendingFees = student.PendingFees,
                FeesStatus = student.FeesStatus.ToString(),
                SchoolName = student.School.Name,
                SchoolLogoUrl = student.School.LogoUrl,
                SchoolAddress = student.School.Address,
                GeneratedAt = DateTime.UtcNow,
            };

            return ApiResponse<StudentReportCardDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report card for student {Id}", studentId);
            return ApiResponse<StudentReportCardDto>.Fail("Failed to generate report", "ERROR");
        }
    }

    public async Task<ApiResponse<AcademicReportDto>> GetAcademicReportAsync(Guid schoolId, string? filterClass = null)
    {
        try
        {
            var query = _uow.Students.Query()
                .Where(s => s.SchoolId == schoolId && !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filterClass))
                query = query.Where(s => s.Class == filterClass);

            var students = await query.ToListAsync();

            var subjects = new[]
            {
                ("Maths",   (Func<Domain.Entities.Student, decimal?>)(s => s.MathMarks)),
                ("Science", s => s.ScienceMarks),
                ("English", s => s.EnglishMarks),
                ("Telugu",  s => s.TeluguMarks),
                ("Social",  s => s.SocialMarks),
            };

            var byClass = students
                .Where(s => s.Class != null)
                .GroupBy(s => s.Class!)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var classStudents = g.ToList();
                    var withPct = classStudents.Where(s => s.Percentage.HasValue).ToList();
                    var passThreshold = 35m;

                    var subjectStats = subjects.Select(sub =>
                    {
                        var marks = classStudents
                            .Where(s => sub.Item2(s).HasValue)
                            .Select(s => sub.Item2(s)!.Value)
                            .ToList();
                        return new SubjectStat
                        {
                            Subject = sub.Item1,
                            AverageMarks = marks.Count > 0 ? Math.Round(marks.Average(), 1) : null,
                            HighestMarks = marks.Count > 0 ? marks.Max() : null,
                            LowestMarks = marks.Count > 0 ? marks.Min() : null,
                            StudentCount = marks.Count,
                        };
                    }).ToList();

                    return new ClassPerformanceDto
                    {
                        Class = g.Key,
                        TotalStudents = classStudents.Count,
                        AveragePercentage = withPct.Count > 0 ? Math.Round(withPct.Average(s => s.Percentage!.Value), 1) : null,
                        HighestPercentage = withPct.Count > 0 ? withPct.Max(s => s.Percentage!.Value) : null,
                        LowestPercentage = withPct.Count > 0 ? withPct.Min(s => s.Percentage!.Value) : null,
                        PassCount = withPct.Count(s => s.Percentage >= passThreshold),
                        PassPercent = withPct.Count > 0
                            ? Math.Round((decimal)withPct.Count(s => s.Percentage >= passThreshold) / withPct.Count * 100, 1)
                            : 0,
                        SubjectStats = subjectStats,
                    };
                })
                .ToList();

            var topPerformers = students
                .Where(s => s.Percentage.HasValue)
                .OrderByDescending(s => s.Percentage)
                .Take(10)
                .Select(s => new TopPerformerDto
                {
                    StudentId = s.Id,
                    StudentName = $"{s.FirstName} {s.LastName}",
                    Class = s.Class,
                    Section = s.Section,
                    Percentage = s.Percentage,
                    Grade = s.Grade,
                })
                .ToList();

            return ApiResponse<AcademicReportDto>.Ok(new AcademicReportDto
            {
                ByClass = byClass,
                TopPerformers = topPerformers,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating academic report for school {Id}", schoolId);
            return ApiResponse<AcademicReportDto>.Fail("Failed to generate report", "ERROR");
        }
    }
}
