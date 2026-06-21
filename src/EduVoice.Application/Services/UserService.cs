using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Users;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, IAuditService auditService, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<UserDto>>> GetUsersAsync(Guid schoolId)
    {
        try
        {
            var users = await _unitOfWork.Users.Query()
                .Where(u => u.SchoolId == schoolId)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            var dtos = users.Select(MapToDto).ToList();
            return ApiResponse<List<UserDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users for school {SchoolId}", schoolId);
            return ApiResponse<List<UserDto>>.Fail("Failed to fetch users", "FETCH_ERROR");
        }
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(Guid schoolId, CreateUserRequest request)
    {
        try
        {
            var existing = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
            if (existing != null)
                return ApiResponse<UserDto>.Fail("Email already registered", "EMAIL_EXISTS");

            var user = new User
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                Email = request.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "USER_CREATED", "User", user.Id.ToString());

            return ApiResponse<UserDto>.Ok(MapToDto(user), "User created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return ApiResponse<UserDto>.Fail("Failed to create user", "CREATE_ERROR");
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid schoolId, Guid userId, UpdateUserRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.Query()
                .Where(u => u.SchoolId == schoolId && u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                return ApiResponse<UserDto>.Fail("User not found", "NOT_FOUND");

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Role = request.Role;
            user.IsActive = request.IsActive;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "USER_UPDATED", "User", userId.ToString());

            return ApiResponse<UserDto>.Ok(MapToDto(user), "User updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return ApiResponse<UserDto>.Fail("Failed to update user", "UPDATE_ERROR");
        }
    }

    public async Task<ApiResponse> DeleteUserAsync(Guid schoolId, Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.Query()
                .Where(u => u.SchoolId == schoolId && u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                return ApiResponse.Fail("User not found", "NOT_FOUND");

            user.IsActive = false;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(schoolId, null, "USER_DELETED", "User", userId.ToString());

            return ApiResponse.Ok("User deactivated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return ApiResponse.Fail("Failed to delete user", "DELETE_ERROR");
        }
    }

    private static UserDto MapToDto(User u) => new()
    {
        Id = u.Id,
        SchoolId = u.SchoolId,
        Email = u.Email,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Role = u.Role,
        IsActive = u.IsActive,
        LastLoginAt = u.LastLoginAt,
        CreatedAt = u.CreatedAt
    };
}
