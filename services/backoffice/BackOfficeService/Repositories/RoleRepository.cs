using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Repositories.Interfaces;

namespace server.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Role?> GetByIdWithUsersAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Role>> GetAllWithUsersCountAsync()
    {
        return await _dbSet
            .Include(r => r.Users)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _dbSet.AnyAsync(r => r.Name == name);
    }

    public async Task<bool> HasUsersAsync(int roleId)
    {
        return await _dbSet
            .Where(r => r.Id == roleId)
            .AnyAsync(r => r.Users.Any());
    }
}
