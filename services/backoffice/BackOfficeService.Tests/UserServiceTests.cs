using Moq;
using server.DTOs.User;
using server.Repositories.Interfaces;
using server.Services;

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

    [Fact]
    public async Task CreateUserAsync_WhenIdentifiantExists_ThrowsInvalidOperationException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.IdentifiantExistsAsync("john"))
            .ReturnsAsync(true);

        var request = new CreateUserRequestDto { Identifiant = "john", Email = "john@example.com" };

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(request));
    }
}