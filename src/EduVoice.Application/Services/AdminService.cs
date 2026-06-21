using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Admin;
using EduVoice.Application.DTOs.Dashboard;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDashboardService _dashboardService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(IUnitOfWork unitOfWork, IDashboardService dashboardService,
        IAuditService auditService, ILogger<AdminService> logger)
    {
        _unitOfWork = unitOfWork;
        _dashboardService = dashboardService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<AdminSchoolDto>>> GetSchoolsAsync(int page, int pageSize, string? search)
    {
        try
        {
            var query = _unitOfWork.Schools.Query();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(lower) ||
                    s.SubDomain.ToLower().Contains(lower) ||
                    s.ContactEmail.ToLower().Contains(lower));
            }

            query = query.OrderByDescending(s => s.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var dtos = new List<AdminSchoolDto>();
            foreach (var school in items)
            {
                var callCount = await _unitOfWork.Calls.CountAsync(c => c.SchoolId == school.Id);
                var userCount = await _unitOfWork.Users.CountAsync(u => u.SchoolId == school.Id);

                dtos.Add(new AdminSchoolDto
                {
                    Id = school.Id,
                    Name = school.Name,
                    SubDomain = school.SubDomain,
                    ContactEmail = school.ContactEmail,
                    ContactPhone = school.ContactPhone,
                    City = school.City,
                    State = school.State,
                    PlanType = school.PlanType,
                    StudentCount = school.StudentCount,
                    IsActive = school.IsActive,
                    TrialEndsAt = school.TrialEndsAt,
                    CreatedAt = school.CreatedAt,
                    TotalCalls = callCount,
                    TotalUsers = userCount
                });
            }

            return ApiResponse<PagedResult<AdminSchoolDto>>.Ok(new PagedResult<AdminSchoolDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching schools");
            return ApiResponse<PagedResult<AdminSchoolDto>>.Fail("Failed to fetch schools", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<AdminSchoolDto>> CreateSchoolAsync(CreateSchoolRequest request)
    {
        try
        {
            var existingSubDomain = await _unitOfWork.Schools.FirstOrDefaultAsync(s => s.SubDomain == request.SubDomain.ToLower());
            if (existingSubDomain != null)
                return ApiResponse<AdminSchoolDto>.Fail("SubDomain already taken", "SUBDOMAIN_TAKEN");

            var existingEmail = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.AdminEmail.ToLower());
            if (existingEmail != null)
                return ApiResponse<AdminSchoolDto>.Fail("Admin email already registered", "EMAIL_EXISTS");

            await _unitOfWork.BeginTransactionAsync();

            var school = new School
            {
                Id = Guid.NewGuid(),
                Name = request.SchoolName,
                SubDomain = request.SubDomain.ToLower(),
                ContactEmail = request.ContactEmail.ToLower(),
                ContactPhone = request.ContactPhone,
                City = request.City,
                State = request.State,
                PlanType = request.PlanType,
                IsActive = true,
                TrialEndsAt = request.TrialEndsAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Schools.AddAsync(school);

            var admin = new User
            {
                Id = Guid.NewGuid(),
                SchoolId = school.Id,
                Email = request.AdminEmail.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword, 12),
                FirstName = request.AdminFirstName,
                LastName = request.AdminLastName,
                Role = UserRole.SchoolAdmin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(admin);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            await _auditService.LogAsync(school.Id, null, "SCHOOL_CREATED_BY_ADMIN", "School", school.Id.ToString());

            return ApiResponse<AdminSchoolDto>.Ok(new AdminSchoolDto
            {
                Id = school.Id,
                Name = school.Name,
                SubDomain = school.SubDomain,
                ContactEmail = school.ContactEmail,
                ContactPhone = school.ContactPhone,
                City = school.City,
                State = school.State,
                PlanType = school.PlanType,
                IsActive = school.IsActive,
                TrialEndsAt = school.TrialEndsAt,
                CreatedAt = school.CreatedAt
            }, "School created successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating school");
            return ApiResponse<AdminSchoolDto>.Fail("Failed to create school", "CREATE_ERROR");
        }
    }

    public async Task<ApiResponse<AdminSchoolDto>> UpdateSchoolAsync(Guid schoolId, CreateSchoolRequest request)
    {
        try
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null)
                return ApiResponse<AdminSchoolDto>.Fail("School not found", "NOT_FOUND");

            school.Name = request.SchoolName;
            school.ContactEmail = request.ContactEmail.ToLower();
            school.ContactPhone = request.ContactPhone;
            school.City = request.City;
            school.State = request.State;
            school.PlanType = request.PlanType;
            school.TrialEndsAt = request.TrialEndsAt;
            school.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Schools.Update(school);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "SCHOOL_UPDATED_BY_ADMIN", "School", schoolId.ToString());

            return ApiResponse<AdminSchoolDto>.Ok(new AdminSchoolDto
            {
                Id = school.Id,
                Name = school.Name,
                SubDomain = school.SubDomain,
                ContactEmail = school.ContactEmail,
                ContactPhone = school.ContactPhone,
                City = school.City,
                State = school.State,
                PlanType = school.PlanType,
                IsActive = school.IsActive,
                TrialEndsAt = school.TrialEndsAt,
                CreatedAt = school.CreatedAt
            }, "School updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating school {SchoolId}", schoolId);
            return ApiResponse<AdminSchoolDto>.Fail("Failed to update school", "UPDATE_ERROR");
        }
    }

    public async Task<ApiResponse> DeleteSchoolAsync(Guid schoolId)
    {
        try
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school == null)
                return ApiResponse.Fail("School not found", "NOT_FOUND");

            school.IsActive = false;
            school.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Schools.Update(school);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "SCHOOL_DEACTIVATED", "School", schoolId.ToString());

            return ApiResponse.Ok("School deactivated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting school {SchoolId}", schoolId);
            return ApiResponse.Fail("Failed to delete school", "DELETE_ERROR");
        }
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetSchoolStatsAsync(Guid schoolId)
    {
        return await _dashboardService.GetStatsAsync(schoolId);
    }
}
