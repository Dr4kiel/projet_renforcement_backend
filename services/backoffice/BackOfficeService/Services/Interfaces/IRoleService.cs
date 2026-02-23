using server.DTOs.Role;

namespace server.Services.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(int id);
    Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request);
    Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequestDto request);
    Task<bool> DeleteRoleAsync(int id);
}
