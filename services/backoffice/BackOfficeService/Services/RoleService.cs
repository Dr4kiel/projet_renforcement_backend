using server.DTOs.Role;
using server.Models;
using server.Repositories.Interfaces;
using server.Services.Interfaces;

namespace server.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetAllWithUsersCountAsync();
        return roles.Select(MapToDto);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        var role = await _roleRepository.GetByIdWithUsersAsync(id);
        return role == null ? null : MapToDto(role);
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request)
    {
        // Validate uniqueness
        if (await _roleRepository.NameExistsAsync(request.Name))
            throw new InvalidOperationException("Role name already exists");

        var role = new Role
        {
            Name = request.Name
        };

        var createdRole = await _roleRepository.CreateAsync(role);
        return MapToDto(createdRole);
    }

    public async Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequestDto request)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return null;

        // Validate name uniqueness if changed
        if (request.Name != role.Name)
        {
            if (await _roleRepository.NameExistsAsync(request.Name))
                throw new InvalidOperationException("Role name already exists");
        }

        role.Name = request.Name;
        await _roleRepository.UpdateAsync(role);

        var updatedRole = await _roleRepository.GetByIdWithUsersAsync(id);
        return MapToDto(updatedRole!);
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        // Prevent deletion if role has users
        if (await _roleRepository.HasUsersAsync(id))
            throw new InvalidOperationException("Cannot delete role with assigned users");

        return await _roleRepository.DeleteAsync(id);
    }

    private static RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            UserCount = role.Users?.Count ?? 0
        };
    }
}
