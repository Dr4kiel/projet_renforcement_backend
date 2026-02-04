using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using server.DTOs.Auth;
using server.Repositories.Interfaces;
using server.Services.Interfaces;

namespace server.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // Find user by identifiant
        var user = await _userRepository.GetByIdentifiantAsync(request.Identifiant);
        if (user == null) return null;

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return null;

        // Generate JWT token
        var token = GenerateJwtToken(user.Id, user.Identifiant, user.Role?.Name);
        var expiresAt = DateTime.UtcNow.AddHours(
            int.Parse(_configuration["Jwt:ExpiryHours"] ?? "24")
        );

        return new LoginResponseDto
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresAt = expiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                Identifiant = user.Identifiant,
                Email = user.Email,
                RoleName = user.Role?.Name
            }
        };
    }

    public string GenerateJwtToken(int userId, string identifiant, string? roleName)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey not configured"));
        var issuer = _configuration["Jwt:Issuer"] ?? "ProductionDashboard";
        var audience = _configuration["Jwt:Audience"] ?? "ProductionDashboardClient";
        var expiryHours = int.Parse(_configuration["Jwt:ExpiryHours"] ?? "24");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, identifiant),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(roleName))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(expiryHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
