using Moq;
using server.DTOs.Role;
using server.Models;
using server.Repositories.Interfaces;
using server.Services;
using FluentAssertions;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _roleService = new RoleService(_roleRepositoryMock.Object);
    }

    #region GetAllRolesAsync Tests

    [Fact]
    public async Task GetAllRolesAsync_ReturnsAllRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role
            {
                Id = 1,
                Name = "Admin",
                Users = new List<User>
                {
                    new User { Id = 1, Identifiant = "user1", Email = "user1@example.com", Password = "hash" },
                    new User { Id = 2, Identifiant = "user2", Email = "user2@example.com", Password = "hash" }
                }
            },
            new Role
            {
                Id = 2,
                Name = "User",
                Users = new List<User>()
            }
        };

        _roleRepositoryMock
            .Setup(r => r.GetAllWithUsersCountAsync())
            .ReturnsAsync(roles);

        // Act
        var result = await _roleService.GetAllRolesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Admin");
        result.First().UserCount.Should().Be(2);
        result.Last().Name.Should().Be("User");
        result.Last().UserCount.Should().Be(0);
    }

    #endregion

    #region GetRoleByIdAsync Tests

    [Fact]
    public async Task GetRoleByIdAsync_WhenRoleExists_ReturnsRole()
    {
        // Arrange
        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            Users = new List<User>
            {
                new User { Id = 1, Identifiant = "user1", Email = "user1@example.com", Password = "hash" }
            }
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdWithUsersAsync(1))
            .ReturnsAsync(role);

        // Act
        var result = await _roleService.GetRoleByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Admin");
        result.UserCount.Should().Be(1);
    }

    [Fact]
    public async Task GetRoleByIdAsync_WhenRoleDoesNotExist_ReturnsNull()
    {
        // Arrange
        _roleRepositoryMock
            .Setup(r => r.GetByIdWithUsersAsync(999))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _roleService.GetRoleByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateRoleAsync Tests

    [Fact]
    public async Task CreateRoleAsync_WhenNameExists_ThrowsInvalidOperationException()
    {
        // Arrange
        _roleRepositoryMock
            .Setup(r => r.NameExistsAsync("Admin"))
            .ReturnsAsync(true);

        var request = new CreateRoleRequestDto { Name = "Admin" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _roleService.CreateRoleAsync(request));
    }

    [Fact]
    public async Task CreateRoleAsync_WithValidData_CreatesRoleSuccessfully()
    {
        // Arrange
        _roleRepositoryMock
            .Setup(r => r.NameExistsAsync("NewRole"))
            .ReturnsAsync(false);

        var createdRole = new Role
        {
            Id = 1,
            Name = "NewRole",
            Users = new List<User>()
        };

        _roleRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Role>()))
            .ReturnsAsync(createdRole);

        var request = new CreateRoleRequestDto { Name = "NewRole" };

        // Act
        var result = await _roleService.CreateRoleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("NewRole");
        result.UserCount.Should().Be(0);
        _roleRepositoryMock.Verify(r => r.CreateAsync(It.Is<Role>(role =>
            role.Name == "NewRole"
        )), Times.Once);
    }

    #endregion

    #region UpdateRoleAsync Tests

    [Fact]
    public async Task UpdateRoleAsync_WhenRoleDoesNotExist_ReturnsNull()
    {
        // Arrange
        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Role?)null);

        var request = new UpdateRoleRequestDto { Name = "NewName" };

        // Act
        var result = await _roleService.UpdateRoleAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRoleAsync_WhenChangingNameToExistingOne_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingRole = new Role
        {
            Id = 1,
            Name = "Admin",
            Users = new List<User>()
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingRole);
        _roleRepositoryMock
            .Setup(r => r.NameExistsAsync("User"))
            .ReturnsAsync(true);

        var request = new UpdateRoleRequestDto { Name = "User" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _roleService.UpdateRoleAsync(1, request));
    }

    [Fact]
    public async Task UpdateRoleAsync_WithSameName_DoesNotCheckUniqueness()
    {
        // Arrange
        var existingRole = new Role
        {
            Id = 1,
            Name = "Admin",
            Users = new List<User>()
        };

        var updatedRole = new Role
        {
            Id = 1,
            Name = "Admin",
            Users = new List<User>()
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingRole);
        _roleRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Role>()))
            .ReturnsAsync(existingRole);
        _roleRepositoryMock
            .Setup(r => r.GetByIdWithUsersAsync(1))
            .ReturnsAsync(updatedRole);

        var request = new UpdateRoleRequestDto { Name = "Admin" };

        // Act
        var result = await _roleService.UpdateRoleAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Admin");
        _roleRepositoryMock.Verify(r => r.NameExistsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRoleAsync_WithValidData_UpdatesRoleSuccessfully()
    {
        // Arrange
        var existingRole = new Role
        {
            Id = 1,
            Name = "Admin",
            Users = new List<User>()
        };

        var updatedRole = new Role
        {
            Id = 1,
            Name = "SuperAdmin",
            Users = new List<User>
            {
                new User { Id = 1, Identifiant = "user1", Email = "user1@example.com", Password = "hash" }
            }
        };

        _roleRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingRole);
        _roleRepositoryMock
            .Setup(r => r.NameExistsAsync("SuperAdmin"))
            .ReturnsAsync(false);
        _roleRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Role>()))
            .ReturnsAsync(existingRole);
        _roleRepositoryMock
            .Setup(r => r.GetByIdWithUsersAsync(1))
            .ReturnsAsync(updatedRole);

        var request = new UpdateRoleRequestDto { Name = "SuperAdmin" };

        // Act
        var result = await _roleService.UpdateRoleAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("SuperAdmin");
        result.UserCount.Should().Be(1);
        _roleRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Role>()), Times.Once);
    }

    #endregion

    #region DeleteRoleAsync Tests

    [Fact]
    public async Task DeleteRoleAsync_WhenRoleHasUsers_ThrowsInvalidOperationException()
    {
        // Arrange
        _roleRepositoryMock
            .Setup(r => r.HasUsersAsync(1))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _roleService.DeleteRoleAsync(1));
        _roleRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRoleAsync_WhenRoleHasNoUsers_DeletesSuccessfully()
    {
        // Arrange
        _roleRepositoryMock
            .Setup(r => r.HasUsersAsync(1))
            .ReturnsAsync(false);
        _roleRepositoryMock
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _roleService.DeleteRoleAsync(1);

        // Assert
        result.Should().BeTrue();
        _roleRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_WhenRoleDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _roleRepositoryMock
            .Setup(r => r.HasUsersAsync(999))
            .ReturnsAsync(false);
        _roleRepositoryMock
            .Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _roleService.DeleteRoleAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
