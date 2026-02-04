using server.DTOs.User;

namespace server.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserRequestDto request);
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequestDto request);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> ChangePasswordAsync(int id, ChangePasswordRequestDto request);
}
