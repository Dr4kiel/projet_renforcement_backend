using Moq;
using server.DTOs.User;
using server.Models;
using server.Repositories.Interfaces;
using server.Services;
using FluentAssertions;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userService = new UserService(_userRepositoryMock.Object, _roleRepositoryMock.Object);
    }

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Identifiant = "user1",
                Email = "user1@example.com",
                Password = "hashedPassword1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RoleId = 1,
                Role = new Role { Id = 1, Name = "Admin" }
            },
            new User
            {
                Id = 2,
                Identifiant = "user2",
                Email = "user2@example.com",
                Password = "hashedPassword2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RoleId = null,
                Role = null
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetAllWithRolesAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Identifiant.Should().Be("user1");
        result.First().RoleName.Should().Be("Admin");
        result.Last().Identifiant.Should().Be("user2");
        result.Last().RoleName.Should().BeNull();
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = "hashedPassword",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RoleId = 1,
            Role = new Role { Id = 1, Name = "Admin" }
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdWithRoleAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Identifiant.Should().Be("john");
        result.RoleName.Should().Be("Admin");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByIdWithRoleAsync(999))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_WhenIdentifiantExists_ThrowsInvalidOperationException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.IdentifiantExistsAsync("john"))
            .ReturnsAsync(true);

        var request = new CreateUserRequestDto
        {
            Identifiant = "john",
            Email = "john@example.com",
            Password = "password123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(request));
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailExists_ThrowsInvalidOperationException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.IdentifiantExistsAsync("john"))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("john@example.com"))
            .ReturnsAsync(true);

        var request = new CreateUserRequestDto
        {
            Identifiant = "john",
            Email = "john@example.com",
            Password = "password123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(request));
    }

    [Fact]
    public async Task CreateUserAsync_WhenRoleDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.IdentifiantExistsAsync("john"))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("john@example.com"))
            .ReturnsAsync(false);
        _roleRepositoryMock
            .Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        var request = new CreateUserRequestDto
        {
            Identifiant = "john",
            Email = "john@example.com",
            Password = "password123",
            RoleId = 999
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(request));
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_CreatesUserSuccessfully()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.IdentifiantExistsAsync("john"))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("john@example.com"))
            .ReturnsAsync(false);
        _roleRepositoryMock
            .Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);

        var createdUser = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
            RoleId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var userWithRole = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = createdUser.Password,
            RoleId = 1,
            Role = new Role { Id = 1, Name = "Admin" },
            CreatedAt = createdUser.CreatedAt,
            UpdatedAt = createdUser.UpdatedAt
        };

        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);
        _userRepositoryMock
            .Setup(r => r.GetByIdWithRoleAsync(1))
            .ReturnsAsync(userWithRole);

        var request = new CreateUserRequestDto
        {
            Identifiant = "john",
            Email = "john@example.com",
            Password = "password123",
            RoleId = 1
        };

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Identifiant.Should().Be("john");
        result.Email.Should().Be("john@example.com");
        result.RoleName.Should().Be("Admin");
        _userRepositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u =>
            u.Identifiant == "john" &&
            u.Email == "john@example.com" &&
            u.RoleId == 1
        )), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithoutRole_CreatesUserSuccessfully()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.IdentifiantExistsAsync("john"))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("john@example.com"))
            .ReturnsAsync(false);

        var createdUser = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
            RoleId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);
        _userRepositoryMock
            .Setup(r => r.GetByIdWithRoleAsync(1))
            .ReturnsAsync(createdUser);

        var request = new CreateUserRequestDto
        {
            Identifiant = "john",
            Email = "john@example.com",
            Password = "password123"
        };

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().BeNull();
        result.RoleName.Should().BeNull();
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((User?)null);

        var request = new UpdateUserRequestDto { Identifiant = "newname" };

        // Act
        var result = await _userService.UpdateUserAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserAsync_WhenChangingIdentifiantToExistingOne_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = "hashedPassword",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingUser);
        _userRepositoryMock
            .Setup(r => r.IdentifiantExistsAsync("jane"))
            .ReturnsAsync(true);

        var request = new UpdateUserRequestDto { Identifiant = "jane" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateUserAsync(1, request));
    }

    [Fact]
    public async Task UpdateUserAsync_WhenChangingEmailToExistingOne_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = "hashedPassword",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingUser);
        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("jane@example.com"))
            .ReturnsAsync(true);

        var request = new UpdateUserRequestDto { Email = "jane@example.com" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateUserAsync(1, request));
    }

    [Fact]
    public async Task UpdateUserAsync_WhenRoleDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = "hashedPassword",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingUser);
        _roleRepositoryMock
            .Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        var request = new UpdateUserRequestDto { RoleId = 999 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateUserAsync(1, request));
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidData_UpdatesUserSuccessfully()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Identifiant = "john",
            Email = "john@example.com",
            Password = "hashedPassword",
            RoleId = null,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedUser = new User
        {
            Id = 1,
            Identifiant = "john_updated",
            Email = "john.updated@example.com",
            Password = "hashedPassword",
            RoleId = 1,
            Role = new Role { Id = 1, Name = "Admin" },
            CreatedAt = existingUser.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingUser);
        _userRepositoryMock
            .Setup(r => r.IdentifiantExistsAsync("john_updated"))
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("john.updated@example.com"))
            .ReturnsAsync(false);
        _roleRepositoryMock
            .Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);
        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(existingUser);
        _userRepositoryMock
            .Setup(r => r.GetByIdWithRoleAsync(1))
            .ReturnsAsync(updatedUser);

        var request = new UpdateUserRequestDto
        {
            Identifiant = "john_updated",
            Email = "john.updated@example.com",
            RoleId = 1
        };

        // Act
        var result = await _userService.UpdateUserAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Identifiant.Should().Be("john_updated");
        result.Email.Should().Be("john.updated@example.com");
        result.RoleName.Should().Be("Admin");
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_ReturnsTrue()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAsync(1);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.DeleteUserAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WhenUserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((User?)null);

        var request = new ChangePasswordRequestDto
        {
            CurrentPassword = "oldPassword",
            NewPassword = "newPassword123"
        };

        // Act
        var result = await _userService.ChangePasswordAsync(999, request);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIsIncorrect_ThrowsInvalidOperationException()
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
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        var request = new ChangePasswordRequestDto
        {
            CurrentPassword = "wrongPassword",
            NewPassword = "newPassword123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _userService.ChangePasswordAsync(1, request));
    }

    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ChangesPasswordSuccessfully()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("oldPassword");
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
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        var request = new ChangePasswordRequestDto
        {
            CurrentPassword = "oldPassword",
            NewPassword = "newPassword123"
        };

        // Act
        var result = await _userService.ChangePasswordAsync(1, request);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.Id == 1 &&
            BCrypt.Net.BCrypt.Verify("newPassword123", u.Password)
        )), Times.Once);
    }

    #endregion
}
