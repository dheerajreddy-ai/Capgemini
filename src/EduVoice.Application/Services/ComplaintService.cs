using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Complaints;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace EduVoice.Application.Services;

public class ComplaintService : IComplaintService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILogger<ComplaintService> _logger;

    public ComplaintService(IUnitOfWork unitOfWork, IAuditService auditService, ILogger<ComplaintService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<ComplaintDto>>> GetComplaintsAsync(Guid schoolId, int page, int pageSize,
        ComplaintStatus? status, ComplaintCategory? category, ComplaintPriority? priority)
    {
        try
        {
            var query = _unitOfWork.Complaints.Query()
                .Include(c => c.Student)
                .Where(c => c.SchoolId == schoolId);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            if (category.HasValue)
                query = query.Where(c => c.Category == category.Value);

            if (priority.HasValue)
                query = query.Where(c => c.Priority == priority.Value);

            query = query.OrderByDescending(c => c.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var dtos = items.Select(c => new ComplaintDto
            {
                Id = c.Id,
                StudentId = c.StudentId,
                StudentName = $"{c.Student.FirstName} {c.Student.LastName}".Trim(),
                Class = c.Student.Class,
                Section = c.Student.Section,
                CallId = c.CallId,
                Category = c.Category,
                Priority = c.Priority,
                Status = c.Status,
                Summary = c.Summary,
                DetailedDescription = c.DetailedDescription,
                ParentName = c.ParentName,
                ParentPhone = c.ParentPhone,
                Resolution = c.Resolution,
                AssignedToUserId = c.AssignedToUserId,
                ResolvedAt = c.ResolvedAt,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return ApiResponse<PagedResult<ComplaintDto>>.Ok(new PagedResult<ComplaintDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching complaints for school {SchoolId}", schoolId);
            return ApiResponse<PagedResult<ComplaintDto>>.Fail("Failed to fetch complaints", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<ComplaintDto>> GetComplaintByIdAsync(Guid schoolId, Guid complaintId)
    {
        try
        {
            var complaint = await _unitOfWork.Complaints.Query()
                .Include(c => c.Student)
                .Where(c => c.SchoolId == schoolId && c.Id == complaintId)
                .FirstOrDefaultAsync();

            if (complaint == null)
                return ApiResponse<ComplaintDto>.Fail("Complaint not found", "NOT_FOUND");

            return ApiResponse<ComplaintDto>.Ok(new ComplaintDto
            {
                Id = complaint.Id,
                StudentId = complaint.StudentId,
                StudentName = $"{complaint.Student.FirstName} {complaint.Student.LastName}".Trim(),
                Class = complaint.Student.Class,
                Section = complaint.Student.Section,
                CallId = complaint.CallId,
                Category = complaint.Category,
                Priority = complaint.Priority,
                Status = complaint.Status,
                Summary = complaint.Summary,
                DetailedDescription = complaint.DetailedDescription,
                ParentName = complaint.ParentName,
                ParentPhone = complaint.ParentPhone,
                Resolution = complaint.Resolution,
                AssignedToUserId = complaint.AssignedToUserId,
                ResolvedAt = complaint.ResolvedAt,
                CreatedAt = complaint.CreatedAt,
                UpdatedAt = complaint.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching complaint {ComplaintId}", complaintId);
            return ApiResponse<ComplaintDto>.Fail("Failed to fetch complaint", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<ComplaintDto>> UpdateComplaintAsync(Guid schoolId, Guid complaintId, UpdateComplaintRequest request)
    {
        try
        {
            var complaint = await _unitOfWork.Complaints.Query()
                .Include(c => c.Student)
                .Where(c => c.SchoolId == schoolId && c.Id == complaintId)
                .FirstOrDefaultAsync();

            if (complaint == null)
                return ApiResponse<ComplaintDto>.Fail("Complaint not found", "NOT_FOUND");

            complaint.Status = request.Status;
            complaint.Priority = request.Priority;
            complaint.Resolution = request.Resolution;
            complaint.AssignedToUserId = request.AssignedToUserId;
            complaint.UpdatedAt = DateTime.UtcNow;

            if (request.Status == ComplaintStatus.Resolved || request.Status == ComplaintStatus.Closed)
                complaint.ResolvedAt ??= DateTime.UtcNow;

            _unitOfWork.Complaints.Update(complaint);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "COMPLAINT_UPDATED", "Complaint", complaintId.ToString());

            return ApiResponse<ComplaintDto>.Ok(new ComplaintDto
            {
                Id = complaint.Id,
                StudentId = complaint.StudentId,
                StudentName = $"{complaint.Student.FirstName} {complaint.Student.LastName}".Trim(),
                Class = complaint.Student.Class,
                Section = complaint.Student.Section,
                CallId = complaint.CallId,
                Category = complaint.Category,
                Priority = complaint.Priority,
                Status = complaint.Status,
                Summary = complaint.Summary,
                DetailedDescription = complaint.DetailedDescription,
                ParentName = complaint.ParentName,
                ParentPhone = complaint.ParentPhone,
                Resolution = complaint.Resolution,
                AssignedToUserId = complaint.AssignedToUserId,
                ResolvedAt = complaint.ResolvedAt,
                CreatedAt = complaint.CreatedAt,
                UpdatedAt = complaint.UpdatedAt
            }, "Complaint updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating complaint {ComplaintId}", complaintId);
            return ApiResponse<ComplaintDto>.Fail("Failed to update complaint", "UPDATE_ERROR");
        }
    }

    public async Task<byte[]> ExportToExcelAsync(Guid schoolId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var complaints = await _unitOfWork.Complaints.Query()
            .Include(c => c.Student)
            .Where(c => c.SchoolId == schoolId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Complaints");

        var headers = new[]
        {
            "Id", "StudentName", "Class", "Section", "ParentName", "ParentPhone",
            "Category", "Priority", "Status", "Summary", "Resolution",
            "CreatedAt", "ResolvedAt"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
        }

        for (int row = 0; row < complaints.Count; row++)
        {
            var c = complaints[row];
            var r = row + 2;
            ws.Cells[r, 1].Value = c.Id.ToString();
            ws.Cells[r, 2].Value = $"{c.Student.FirstName} {c.Student.LastName}".Trim();
            ws.Cells[r, 3].Value = c.Student.Class;
            ws.Cells[r, 4].Value = c.Student.Section;
            ws.Cells[r, 5].Value = c.ParentName;
            ws.Cells[r, 6].Value = c.ParentPhone;
            ws.Cells[r, 7].Value = c.Category.ToString();
            ws.Cells[r, 8].Value = c.Priority.ToString();
            ws.Cells[r, 9].Value = c.Status.ToString();
            ws.Cells[r, 10].Value = c.Summary;
            ws.Cells[r, 11].Value = c.Resolution;
            ws.Cells[r, 12].Value = c.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            ws.Cells[r, 13].Value = c.ResolvedAt?.ToString("yyyy-MM-dd HH:mm");
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }
}
