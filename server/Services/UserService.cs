using server.DTOs.User;
using server.Models;
using server.Repositories.Interfaces;
using server.Services.Interfaces;

namespace server.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllWithRolesAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdWithRoleAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequestDto request)
    {
        // Validate uniqueness
        if (await _userRepository.IdentifiantExistsAsync(request.Identifiant))
            throw new InvalidOperationException("Identifiant already exists");

        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new InvalidOperationException("Email already exists");

        // Validate role if provided
        if (request.RoleId.HasValue)
        {
            if (!await _roleRepository.ExistsAsync(request.RoleId.Value))
                throw new InvalidOperationException("Role not found");
        }

        // Hash password
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Identifiant = request.Identifiant,
            Password = hashedPassword,
            Email = request.Email,
            RoleId = request.RoleId,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);
        var userWithRole = await _userRepository.GetByIdWithRoleAsync(createdUser.Id);
        return MapToDto(userWithRole!);
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        // Validate identifiant uniqueness if changed
        if (request.Identifiant != null && request.Identifiant != user.Identifiant)
        {
            if (await _userRepository.IdentifiantExistsAsync(request.Identifiant))
                throw new InvalidOperationException("Identifiant already exists");
            user.Identifiant = request.Identifiant;
        }

        // Validate email uniqueness if changed
        if (request.Email != null && request.Email != user.Email)
        {
            if (await _userRepository.EmailExistsAsync(request.Email))
                throw new InvalidOperationException("Email already exists");
            user.Email = request.Email;
        }

        // Validate role if provided
        if (request.RoleId.HasValue)
        {
            if (!await _roleRepository.ExistsAsync(request.RoleId.Value))
                throw new InvalidOperationException("Role not found");
            user.RoleId = request.RoleId;
        }

        await _userRepository.UpdateAsync(user);
        var updatedUser = await _userRepository.GetByIdWithRoleAsync(id);
        return MapToDto(updatedUser!);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    public async Task<bool> ChangePasswordAsync(int id, ChangePasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
            throw new InvalidOperationException("Current password is incorrect");

        // Hash and update password
        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);
        return true;
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Identifiant = user.Identifiant,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name
        };
    }
}
