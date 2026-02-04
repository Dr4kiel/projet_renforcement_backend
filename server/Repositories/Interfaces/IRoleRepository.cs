using server.Models;

namespace server.Repositories.Interfaces;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> GetByIdWithUsersAsync(int id);
    Task<IEnumerable<Role>> GetAllWithUsersCountAsync();
    Task<bool> NameExistsAsync(string name);
    Task<bool> HasUsersAsync(int roleId);
}
