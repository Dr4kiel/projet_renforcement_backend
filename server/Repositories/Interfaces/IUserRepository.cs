using server.Models;

namespace server.Repositories.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByIdentifiantAsync(string identifiant);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithRoleAsync(int id);
    Task<IEnumerable<User>> GetAllWithRolesAsync();
    Task<bool> IdentifiantExistsAsync(string identifiant);
    Task<bool> EmailExistsAsync(string email);
}
