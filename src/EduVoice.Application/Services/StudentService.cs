using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Students;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace EduVoice.Application.Services;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILowMarksAlertService _lowMarksAlertService;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IUnitOfWork unitOfWork, IAuditService auditService,
        ILowMarksAlertService lowMarksAlertService, ILogger<StudentService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _lowMarksAlertService = lowMarksAlertService;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<StudentListDto>>> GetStudentsAsync(Guid schoolId, int page, int pageSize,
        string? search, string? classFilter, string? sectionFilter, FeesStatus? feesStatus, string? sortBy)
    {
        try
        {
            var query = _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(s =>
                    s.FirstName.ToLower().Contains(lowerSearch) ||
                    s.LastName.ToLower().Contains(lowerSearch) ||
                    s.StudentId.ToLower().Contains(lowerSearch) ||
                    s.ParentName.ToLower().Contains(lowerSearch) ||
                    s.ParentPhone.Contains(lowerSearch));
            }

            if (!string.IsNullOrWhiteSpace(classFilter))
                query = query.Where(s => s.Class == classFilter);

            if (!string.IsNullOrWhiteSpace(sectionFilter))
                query = query.Where(s => s.Section == sectionFilter);

            if (feesStatus.HasValue)
                query = query.Where(s => s.FeesStatus == feesStatus.Value);

            query = sortBy?.ToLower() switch
            {
                "name" => query.OrderBy(s => s.FirstName).ThenBy(s => s.LastName),
                "fees" => query.OrderByDescending(s => s.PendingFees),
                "class" => query.OrderBy(s => s.Class).ThenBy(s => s.Section),
                "attendance" => query.OrderByDescending(s => s.AttendancePercentage),
                _ => query.OrderByDescending(s => s.CreatedAt)
            };

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var dtos = items.Select(MapToListDto).ToList();

            return ApiResponse<PagedResult<StudentListDto>>.Ok(new PagedResult<StudentListDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching students for school {SchoolId}", schoolId);
            return ApiResponse<PagedResult<StudentListDto>>.Fail("Failed to fetch students", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<StudentDto>> GetStudentByIdAsync(Guid schoolId, Guid studentId)
    {
        try
        {
            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && s.Id == studentId && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (student == null)
                return ApiResponse<StudentDto>.Fail("Student not found", "NOT_FOUND");

            return ApiResponse<StudentDto>.Ok(MapToDto(student));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching student {StudentId}", studentId);
            return ApiResponse<StudentDto>.Fail("Failed to fetch student", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<StudentDto>> CreateStudentAsync(Guid schoolId, CreateStudentRequest request)
    {
        try
        {
            var existing = await _unitOfWork.Students.FirstOrDefaultAsync(s =>
                s.SchoolId == schoolId && s.StudentId == request.StudentId && !s.IsDeleted);

            if (existing != null)
                return ApiResponse<StudentDto>.Fail("Student ID already exists", "STUDENT_ID_EXISTS");

            var student = new Student
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                StudentId = request.StudentId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Class = request.Class,
                Section = request.Section,
                ParentName = request.ParentName,
                ParentPhone = request.ParentPhone,
                ParentPhone2 = request.ParentPhone2,
                ParentEmail = request.ParentEmail,
                Address = request.Address,
                MathMarks = request.MathMarks,
                ScienceMarks = request.ScienceMarks,
                EnglishMarks = request.EnglishMarks,
                TeluguMarks = request.TeluguMarks,
                SocialMarks = request.SocialMarks,
                MaxMarks = request.MaxMarks,
                AttendancePresentDays = request.AttendancePresentDays,
                AttendanceTotalDays = request.AttendanceTotalDays,
                TotalFees = request.TotalFees,
                PaidFees = request.PaidFees,
                PendingFees = request.TotalFees - request.PaidFees,
                FeesStatus = request.FeesStatus,
                FeesDueDate = request.FeesDueDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            CalculateDerivedFields(student);

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "STUDENT_CREATED", "Student", student.Id.ToString());

            return ApiResponse<StudentDto>.Ok(MapToDto(student), "Student created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student");
            return ApiResponse<StudentDto>.Fail("Failed to create student", "CREATE_ERROR");
        }
    }

    public async Task<ApiResponse<StudentDto>> UpdateStudentAsync(Guid schoolId, Guid studentId, UpdateStudentRequest request)
    {
        try
        {
            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && s.Id == studentId && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (student == null)
                return ApiResponse<StudentDto>.Fail("Student not found", "NOT_FOUND");

            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.Class = request.Class;
            student.Section = request.Section;
            student.ParentName = request.ParentName;
            student.ParentPhone = request.ParentPhone;
            student.ParentPhone2 = request.ParentPhone2;
            student.ParentEmail = request.ParentEmail;
            student.Address = request.Address;
            student.MathMarks = request.MathMarks;
            student.ScienceMarks = request.ScienceMarks;
            student.EnglishMarks = request.EnglishMarks;
            student.TeluguMarks = request.TeluguMarks;
            student.SocialMarks = request.SocialMarks;
            student.MaxMarks = request.MaxMarks;
            student.AttendancePresentDays = request.AttendancePresentDays;
            student.AttendanceTotalDays = request.AttendanceTotalDays;
            student.TotalFees = request.TotalFees;
            student.PaidFees = request.PaidFees;
            student.PendingFees = request.TotalFees - request.PaidFees;
            student.FeesStatus = request.FeesStatus;
            student.FeesDueDate = request.FeesDueDate;
            student.LastPaymentDate = request.LastPaymentDate;
            student.UpdatedAt = DateTime.UtcNow;

            CalculateDerivedFields(student);

            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "STUDENT_UPDATED", "Student", studentId.ToString());

            // Fire-and-forget low marks alert (checks threshold internally)
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school is not null)
                _ = Task.Run(() => _lowMarksAlertService.CheckAndAlertAsync(student, school));

            return ApiResponse<StudentDto>.Ok(MapToDto(student), "Student updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student {StudentId}", studentId);
            return ApiResponse<StudentDto>.Fail("Failed to update student", "UPDATE_ERROR");
        }
    }

    public async Task<ApiResponse> DeleteStudentAsync(Guid schoolId, Guid studentId)
    {
        try
        {
            var student = await _unitOfWork.Students.Query()
                .Where(s => s.SchoolId == schoolId && s.Id == studentId && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (student == null)
                return ApiResponse.Fail("Student not found", "NOT_FOUND");

            student.IsDeleted = true;
            student.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Students.Update(student);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "STUDENT_DELETED", "Student", studentId.ToString());

            return ApiResponse.Ok("Student deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student {StudentId}", studentId);
            return ApiResponse.Fail("Failed to delete student", "DELETE_ERROR");
        }
    }

    public async Task<ApiResponse<ImportStudentsResult>> ImportFromExcelAsync(Guid schoolId, IFormFile file)
    {
        var result = new ImportStudentsResult();

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                return ApiResponse<ImportStudentsResult>.Fail("Excel file has no worksheets", "INVALID_FILE");

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount < 2)
                return ApiResponse<ImportStudentsResult>.Fail("Excel file has no data rows", "NO_DATA");

            result.TotalRows = rowCount - 1;

            var headers = new Dictionary<string, int>();
            for (int col = 1; col <= (worksheet.Dimension?.Columns ?? 0); col++)
            {
                var header = worksheet.Cells[1, col].Text?.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(header))
                    headers[header] = col;
            }

            var studentsToInsert = new List<Student>();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    string GetCell(string colName) =>
                        headers.TryGetValue(colName, out var col)
                            ? worksheet.Cells[row, col].Text?.Trim() ?? string.Empty
                            : string.Empty;

                    var studentId = GetCell("studentid") ?? GetCell("student id") ?? GetCell("id");
                    var firstName = GetCell("firstname") ?? GetCell("first name");
                    var lastName = GetCell("lastname") ?? GetCell("last name");
                    var parentPhone = GetCell("parentphone") ?? GetCell("parent phone");
                    var parentName = GetCell("parentname") ?? GetCell("parent name");

                    if (string.IsNullOrWhiteSpace(studentId))
                    {
                        result.Errors.Add(new ImportRowError { Row = row, Error = "Student ID is required" });
                        result.FailureCount++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(firstName))
                    {
                        result.Errors.Add(new ImportRowError { Row = row, StudentId = studentId, Error = "First name is required" });
                        result.FailureCount++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(parentPhone))
                    {
                        result.Errors.Add(new ImportRowError { Row = row, StudentId = studentId, Error = "Parent phone is required" });
                        result.FailureCount++;
                        continue;
                    }

                    var existing = await _unitOfWork.Students.FirstOrDefaultAsync(s =>
                        s.SchoolId == schoolId && s.StudentId == studentId && !s.IsDeleted);
                    if (existing != null)
                    {
                        result.Errors.Add(new ImportRowError { Row = row, StudentId = studentId, Error = "Student ID already exists" });
                        result.FailureCount++;
                        continue;
                    }

                    decimal.TryParse(GetCell("totalfees"), out var totalFees);
                    decimal.TryParse(GetCell("paidfees"), out var paidFees);
                    decimal.TryParse(GetCell("mathmarks"), out var mathMarks);
                    decimal.TryParse(GetCell("sciencemarks"), out var scienceMarks);
                    decimal.TryParse(GetCell("englishmarks"), out var englishMarks);
                    decimal.TryParse(GetCell("telugumarks"), out var teluguMarks);
                    decimal.TryParse(GetCell("socialmarks"), out var socialMarks);
                    decimal.TryParse(GetCell("maxmarks"), out var maxMarks);
                    int.TryParse(GetCell("attendancepresentdays"), out var presentDays);
                    int.TryParse(GetCell("attendancetotaldays"), out var totalDays);

                    Enum.TryParse<FeesStatus>(GetCell("feesstatus"), true, out var feesStatusEnum);

                    var student = new Student
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = schoolId,
                        StudentId = studentId,
                        FirstName = firstName,
                        LastName = lastName,
                        Class = GetCell("class"),
                        Section = GetCell("section"),
                        ParentName = string.IsNullOrWhiteSpace(parentName) ? "Parent" : parentName,
                        ParentPhone = parentPhone,
                        ParentEmail = GetCell("parentemail"),
                        Address = GetCell("address"),
                        MathMarks = mathMarks > 0 ? mathMarks : null,
                        ScienceMarks = scienceMarks > 0 ? scienceMarks : null,
                        EnglishMarks = englishMarks > 0 ? englishMarks : null,
                        TeluguMarks = teluguMarks > 0 ? teluguMarks : null,
                        SocialMarks = socialMarks > 0 ? socialMarks : null,
                        MaxMarks = maxMarks > 0 ? maxMarks : null,
                        AttendancePresentDays = presentDays > 0 ? presentDays : null,
                        AttendanceTotalDays = totalDays > 0 ? totalDays : null,
                        TotalFees = totalFees,
                        PaidFees = paidFees,
                        PendingFees = totalFees - paidFees,
                        FeesStatus = feesStatusEnum,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    CalculateDerivedFields(student);
                    studentsToInsert.Add(student);
                    result.SuccessCount++;
                }
                catch (Exception rowEx)
                {
                    _logger.LogWarning(rowEx, "Error processing row {Row}", row);
                    result.Errors.Add(new ImportRowError { Row = row, Error = $"Processing error: {rowEx.Message}" });
                    result.FailureCount++;
                }
            }

            foreach (var student in studentsToInsert)
                await _unitOfWork.Students.AddAsync(student);

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "STUDENTS_IMPORTED", "Student",
                newValues: $"Imported {result.SuccessCount} students");

            return ApiResponse<ImportStudentsResult>.Ok(result, $"Import completed: {result.SuccessCount} success, {result.FailureCount} failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing students for school {SchoolId}", schoolId);
            return ApiResponse<ImportStudentsResult>.Fail("Import failed", "IMPORT_ERROR");
        }
    }

    public async Task<byte[]> ExportToExcelAsync(Guid schoolId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var students = await _unitOfWork.Students.Query()
            .Where(s => s.SchoolId == schoolId && !s.IsDeleted)
            .OrderBy(s => s.Class).ThenBy(s => s.Section).ThenBy(s => s.FirstName)
            .ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Students");

        var headers = new[]
        {
            "StudentId", "FirstName", "LastName", "Class", "Section",
            "ParentName", "ParentPhone", "ParentPhone2", "ParentEmail",
            "MathMarks", "ScienceMarks", "EnglishMarks", "TeluguMarks", "SocialMarks",
            "TotalMarks", "MaxMarks", "Percentage", "Grade",
            "AttendancePresentDays", "AttendanceTotalDays", "AttendancePercentage",
            "TotalFees", "PaidFees", "PendingFees", "FeesStatus", "FeesDueDate"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
        }

        for (int row = 0; row < students.Count; row++)
        {
            var s = students[row];
            var r = row + 2;
            ws.Cells[r, 1].Value = s.StudentId;
            ws.Cells[r, 2].Value = s.FirstName;
            ws.Cells[r, 3].Value = s.LastName;
            ws.Cells[r, 4].Value = s.Class;
            ws.Cells[r, 5].Value = s.Section;
            ws.Cells[r, 6].Value = s.ParentName;
            ws.Cells[r, 7].Value = s.ParentPhone;
            ws.Cells[r, 8].Value = s.ParentPhone2;
            ws.Cells[r, 9].Value = s.ParentEmail;
            ws.Cells[r, 10].Value = s.MathMarks;
            ws.Cells[r, 11].Value = s.ScienceMarks;
            ws.Cells[r, 12].Value = s.EnglishMarks;
            ws.Cells[r, 13].Value = s.TeluguMarks;
            ws.Cells[r, 14].Value = s.SocialMarks;
            ws.Cells[r, 15].Value = s.TotalMarks;
            ws.Cells[r, 16].Value = s.MaxMarks;
            ws.Cells[r, 17].Value = s.Percentage;
            ws.Cells[r, 18].Value = s.Grade;
            ws.Cells[r, 19].Value = s.AttendancePresentDays;
            ws.Cells[r, 20].Value = s.AttendanceTotalDays;
            ws.Cells[r, 21].Value = s.AttendancePercentage;
            ws.Cells[r, 22].Value = s.TotalFees;
            ws.Cells[r, 23].Value = s.PaidFees;
            ws.Cells[r, 24].Value = s.PendingFees;
            ws.Cells[r, 25].Value = s.FeesStatus.ToString();
            ws.Cells[r, 26].Value = s.FeesDueDate?.ToString("yyyy-MM-dd");
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    private static void CalculateDerivedFields(Student student)
    {
        if (student.MathMarks.HasValue || student.ScienceMarks.HasValue ||
            student.EnglishMarks.HasValue || student.TeluguMarks.HasValue || student.SocialMarks.HasValue)
        {
            student.TotalMarks = (student.MathMarks ?? 0) + (student.ScienceMarks ?? 0) +
                (student.EnglishMarks ?? 0) + (student.TeluguMarks ?? 0) + (student.SocialMarks ?? 0);
        }

        if (student.TotalMarks.HasValue && student.MaxMarks.HasValue && student.MaxMarks > 0)
        {
            student.Percentage = Math.Round(student.TotalMarks.Value / student.MaxMarks.Value * 100, 2);
            student.Grade = student.Percentage >= 90 ? "A+" :
                student.Percentage >= 80 ? "A" :
                student.Percentage >= 70 ? "B+" :
                student.Percentage >= 60 ? "B" :
                student.Percentage >= 50 ? "C" :
                student.Percentage >= 35 ? "D" : "F";
        }

        if (student.AttendancePresentDays.HasValue && student.AttendanceTotalDays.HasValue && student.AttendanceTotalDays > 0)
            student.AttendancePercentage = Math.Round((decimal)student.AttendancePresentDays.Value / student.AttendanceTotalDays.Value * 100, 2);
    }

    private static StudentDto MapToDto(Student s) => new()
    {
        Id = s.Id,
        StudentId = s.StudentId,
        FirstName = s.FirstName,
        LastName = s.LastName,
        Class = s.Class,
        Section = s.Section,
        ParentName = s.ParentName,
        ParentPhone = s.ParentPhone,
        ParentPhone2 = s.ParentPhone2,
        ParentEmail = s.ParentEmail,
        Address = s.Address,
        MathMarks = s.MathMarks,
        ScienceMarks = s.ScienceMarks,
        EnglishMarks = s.EnglishMarks,
        TeluguMarks = s.TeluguMarks,
        SocialMarks = s.SocialMarks,
        TotalMarks = s.TotalMarks,
        MaxMarks = s.MaxMarks,
        Percentage = s.Percentage,
        Grade = s.Grade,
        Remarks = s.Remarks,
        AttendancePresentDays = s.AttendancePresentDays,
        AttendanceTotalDays = s.AttendanceTotalDays,
        AttendancePercentage = s.AttendancePercentage,
        TotalFees = s.TotalFees,
        PaidFees = s.PaidFees,
        PendingFees = s.PendingFees,
        FeesStatus = s.FeesStatus,
        FeesDueDate = s.FeesDueDate,
        LastPaymentDate = s.LastPaymentDate,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };

    private static StudentListDto MapToListDto(Student s) => new()
    {
        Id = s.Id,
        StudentId = s.StudentId,
        FirstName = s.FirstName,
        LastName = s.LastName,
        Class = s.Class,
        Section = s.Section,
        ParentName = s.ParentName,
        ParentPhone = s.ParentPhone,
        PendingFees = s.PendingFees,
        FeesStatus = s.FeesStatus,
        AttendancePercentage = s.AttendancePercentage,
        FeesDueDate = s.FeesDueDate,
        CreatedAt = s.CreatedAt
    };
}
