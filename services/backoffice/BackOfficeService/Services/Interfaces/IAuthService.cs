using server.DTOs.Auth;

namespace server.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    string GenerateJwtToken(int userId, string identifiant, string? roleName);
}
