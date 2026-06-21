using System.Security.Claims;
using EduVoice.Domain.Entities;

namespace EduVoice.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    DateTime GetAccessTokenExpiry();
}
