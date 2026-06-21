using EduVoice.Application.Common;
using EduVoice.Application.DTOs.Auth;
using EduVoice.Application.Interfaces;
using EduVoice.Domain.Entities;
using EduVoice.Domain.Enums;
using EduVoice.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EduVoice.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService,
        IEmailService emailService, IAuditService auditService, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _emailService = emailService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponse>> RegisterSchoolAsync(RegisterSchoolRequest request)
    {
        try
        {
            var existingSubDomain = await _unitOfWork.Schools.FirstOrDefaultAsync(s => s.SubDomain == request.SubDomain.ToLower());
            if (existingSubDomain != null)
                return ApiResponse<LoginResponse>.Fail("SubDomain already taken", "SUBDOMAIN_TAKEN");

            var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.AdminEmail.ToLower());
            if (existingUser != null)
                return ApiResponse<LoginResponse>.Fail("Email already registered", "EMAIL_EXISTS");

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
                PlanType = PlanType.Starter,
                IsActive = true,
                TrialEndsAt = DateTime.UtcNow.AddDays(14),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Schools.AddAsync(school);

            var user = new User
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

            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = BCrypt.Net.BCrypt.HashPassword(refreshToken, 12);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            var accessToken = _jwtService.GenerateAccessToken(user);

            await _auditService.LogAsync(school.Id, user.Id, "SCHOOL_REGISTERED", "School", school.Id.ToString());

            return ApiResponse<LoginResponse>.Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _jwtService.GetAccessTokenExpiry(),
                UserId = user.Id,
                SchoolId = school.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                SchoolName = school.Name,
                SchoolSubDomain = school.SubDomain
            }, "School registered successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error registering school");
            return ApiResponse<LoginResponse>.Fail("Registration failed", "REGISTRATION_ERROR");
        }
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return ApiResponse<LoginResponse>.Fail("Invalid email or password", "INVALID_CREDENTIALS");

            if (!user.IsActive)
                return ApiResponse<LoginResponse>.Fail("Account is disabled", "ACCOUNT_DISABLED");

            if (user.SchoolId.HasValue)
            {
                var school = await _unitOfWork.Schools.GetByIdAsync(user.SchoolId.Value);
                if (school != null && !school.IsActive)
                    return ApiResponse<LoginResponse>.Fail("School account is disabled", "SCHOOL_DISABLED");
            }

            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = BCrypt.Net.BCrypt.HashPassword(refreshToken, 12);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var accessToken = _jwtService.GenerateAccessToken(user);

            string? schoolName = null;
            string? schoolSubDomain = null;
            if (user.SchoolId.HasValue)
            {
                var school = await _unitOfWork.Schools.GetByIdAsync(user.SchoolId.Value);
                schoolName = school?.Name;
                schoolSubDomain = school?.SubDomain;
            }

            await _auditService.LogAsync(user.SchoolId, user.Id, "LOGIN", "User", user.Id.ToString());

            return ApiResponse<LoginResponse>.Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _jwtService.GetAccessTokenExpiry(),
                UserId = user.Id,
                SchoolId = user.SchoolId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                SchoolName = schoolName,
                SchoolSubDomain = schoolSubDomain
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return ApiResponse<LoginResponse>.Fail("Login failed", "LOGIN_ERROR");
        }
    }

    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            var users = await _unitOfWork.Users.FindAsync(u =>
                u.RefreshTokenExpiresAt > DateTime.UtcNow);

            User? matchedUser = null;
            foreach (var u in users)
            {
                if (u.RefreshToken != null && BCrypt.Net.BCrypt.Verify(request.RefreshToken, u.RefreshToken))
                {
                    matchedUser = u;
                    break;
                }
            }

            if (matchedUser == null)
                return ApiResponse<LoginResponse>.Fail("Invalid or expired refresh token", "TOKEN_EXPIRED");

            var newRefreshToken = _jwtService.GenerateRefreshToken();
            matchedUser.RefreshToken = BCrypt.Net.BCrypt.HashPassword(newRefreshToken, 12);
            matchedUser.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            _unitOfWork.Users.Update(matchedUser);
            await _unitOfWork.SaveChangesAsync();

            var accessToken = _jwtService.GenerateAccessToken(matchedUser);

            string? schoolName = null;
            string? schoolSubDomain = null;
            if (matchedUser.SchoolId.HasValue)
            {
                var school = await _unitOfWork.Schools.GetByIdAsync(matchedUser.SchoolId.Value);
                schoolName = school?.Name;
                schoolSubDomain = school?.SubDomain;
            }

            return ApiResponse<LoginResponse>.Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = _jwtService.GetAccessTokenExpiry(),
                UserId = matchedUser.Id,
                SchoolId = matchedUser.SchoolId,
                Email = matchedUser.Email,
                FirstName = matchedUser.FirstName,
                LastName = matchedUser.LastName,
                Role = matchedUser.Role,
                SchoolName = schoolName,
                SchoolSubDomain = schoolSubDomain
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return ApiResponse<LoginResponse>.Fail("Token refresh failed", "TOKEN_REFRESH_ERROR");
        }
    }

    public async Task<ApiResponse> LogoutAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse.Fail("User not found", "NOT_FOUND");

            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.Ok("Logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return ApiResponse.Fail("Logout failed", "LOGOUT_ERROR");
        }
    }

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
            if (user == null)
                return ApiResponse.Ok("If the email exists, a reset link has been sent");

            var resetToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            user.PasswordResetToken = BCrypt.Net.BCrypt.HashPassword(resetToken, 12);
            user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(1);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            string schoolName = "EduVoice";
            if (user.SchoolId.HasValue)
            {
                var school = await _unitOfWork.Schools.GetByIdAsync(user.SchoolId.Value);
                schoolName = school?.Name ?? "EduVoice";
            }

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, schoolName);

            return ApiResponse.Ok("If the email exists, a reset link has been sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for {Email}", request.Email);
            return ApiResponse.Fail("Failed to process request", "FORGOT_PASSWORD_ERROR");
        }
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u =>
                u.Email == request.Email.ToLower() &&
                u.PasswordResetExpiresAt > DateTime.UtcNow);

            if (user == null)
                return ApiResponse.Fail("Invalid or expired reset token", "TOKEN_EXPIRED");

            if (user.PasswordResetToken == null || !BCrypt.Net.BCrypt.Verify(request.Token, user.PasswordResetToken))
                return ApiResponse.Fail("Invalid reset token", "TOKEN_INVALID");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
            user.PasswordResetToken = null;
            user.PasswordResetExpiresAt = null;
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(user.SchoolId, user.Id, "PASSWORD_RESET", "User", user.Id.ToString());

            return ApiResponse.Ok("Password reset successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for {Email}", request.Email);
            return ApiResponse.Fail("Password reset failed", "RESET_ERROR");
        }
    }
}
