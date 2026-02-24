using Moq;
using Microsoft.Extensions.Configuration;
using server.DTOs.Auth;
using server.Models;
using server.Repositories.Interfaces;
using server.Services;
using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configurationMock = new Mock<IConfiguration>();

        // Setup default configuration values
        _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns("this-is-a-test-secret-key-with-at-least-32-characters");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configurationMock.Setup(c => c["Jwt:ExpiryHours"]).Returns("24");

        _authService = new AuthService(_userRepositoryMock.Object, _configurationMock.Object);
    }

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByIdentifiantAsync("nonexistent"))
            .ReturnsAsync((User?)null);

        var request = new LoginRequestDto
        {
            Identifiant = "nonexistent",
            Password = "password123"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ReturnsNull()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctPassword");
        var user = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdentifiantAsync("john"))
            .ReturnsAsync(user);

        var request = new LoginRequestDto
        {
            Identifiant = "john",
            Password = "wrongPassword"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = hashedPassword,
            RoleId = 1,
            Role = new Role { Id = 1, Name = "Admin" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdentifiantAsync("john"))
            .ReturnsAsync(user);

        var request = new LoginRequestDto
        {
            Identifiant = "john",
            Password = "password123"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.TokenType.Should().Be("Bearer");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromSeconds(5));
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(1);
        result.User.Identifiant.Should().Be("john");
        result.User.Email.Should().Be("john@example.com");
        result.User.RoleName.Should().Be("Admin");
    }

    [Fact]
    public async Task LoginAsync_WithUserWithoutRole_ReturnsLoginResponseWithoutRole()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = hashedPassword,
            RoleId = null,
            Role = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdentifiantAsync("john"))
            .ReturnsAsync(user);

        var request = new LoginRequestDto
        {
            Identifiant = "john",
            Password = "password123"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.User.RoleName.Should().BeNull();
    }

    #endregion

    #region GenerateJwtToken Tests

    [Fact]
    public void GenerateJwtToken_WithValidParameters_ReturnsValidToken()
    {
        // Arrange
        int userId = 1;
        string identifiant = "john";
        string roleName = "Admin";

        // Act
        var token = _authService.GenerateJwtToken(userId, identifiant, roleName);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Decode and verify token claims
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // JWT uses short claim names when serializing
        jwtToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == "1");
        jwtToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "john");
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == "sub" && c.Value == "1");
        jwtToken.Claims.Should().Contain(c => c.Type == "jti");

        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateJwtToken_WithoutRole_ReturnsTokenWithoutRoleClaim()
    {
        // Arrange
        int userId = 1;
        string identifiant = "john";
        string? roleName = null;

        // Act
        var token = _authService.GenerateJwtToken(userId, identifiant, roleName);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == "1");
        jwtToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "john");
        jwtToken.Claims.Should().NotContain(c => c.Type == "role");
    }

    [Fact]
    public void GenerateJwtToken_WithEmptyRole_ReturnsTokenWithoutRoleClaim()
    {
        // Arrange
        int userId = 1;
        string identifiant = "john";
        string? roleName = "";

        // Act
        var token = _authService.GenerateJwtToken(userId, identifiant, roleName);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().NotContain(c => c.Type == "role");
    }

    [Fact]
    public void GenerateJwtToken_WhenSecretKeyNotConfigured_ThrowsInvalidOperationException()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:SecretKey"]).Returns((string?)null);
        configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        configMock.Setup(c => c["Jwt:ExpiryHours"]).Returns("24");

        var authService = new AuthService(_userRepositoryMock.Object, configMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            authService.GenerateJwtToken(1, "john", "Admin"));
    }

    [Fact]
    public void GenerateJwtToken_WithCustomExpiryHours_ReturnsTokenWithCorrectExpiry()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:SecretKey"]).Returns("this-is-a-test-secret-key-with-at-least-32-characters");
        configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        configMock.Setup(c => c["Jwt:ExpiryHours"]).Returns("48");

        var authService = new AuthService(_userRepositoryMock.Object, configMock.Object);

        // Act
        var token = authService.GenerateJwtToken(1, "john", "Admin");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(48), TimeSpan.FromSeconds(5));
    }

    #endregion
}
