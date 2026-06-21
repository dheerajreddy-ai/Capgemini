using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Users;

namespace EduVoice.Application.Interfaces;

public interface IUserService
{
    Task<ApiResponse<List<UserDto>>> GetUsersAsync(Guid schoolId);
    Task<ApiResponse<UserDto>> CreateUserAsync(Guid schoolId, CreateUserRequest request);
    Task<ApiResponse<UserDto>> UpdateUserAsync(Guid schoolId, Guid userId, UpdateUserRequest request);
    Task<ApiResponse> DeleteUserAsync(Guid schoolId, Guid userId);
}
