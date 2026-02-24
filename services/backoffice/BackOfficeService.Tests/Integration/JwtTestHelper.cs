using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Génère des JWT pour les tests d'intégration.
/// Les constantes doivent correspondre à celles configurées dans CustomWebApplicationFactory.
/// </summary>
public static class JwtTestHelper
{
    // Ces constantes doivent correspondre exactement aux valeurs de appsettings.Testing.json
    public const string SecretKey = "test-secret-key-for-integration-tests-at-least-32-chars";
    public const string Issuer = "TestIssuer";
    public const string Audience = "TestAudience";

    /// <summary>
    /// Génère un Bearer token avec le rôle spécifié.
    /// </summary>
    public static string GenerateToken(int userId = 1, string identifiant = "testuser", string? role = "Admin")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, identifiant),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
