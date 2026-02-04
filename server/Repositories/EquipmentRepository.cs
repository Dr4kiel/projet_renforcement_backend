using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Repositories.Interfaces;

namespace server.Repositories;

public class EquipmentRepository : GenericRepository<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Equipment?> GetByIdWithRelationsAsync(int id)
    {
        return await _dbSet
            .Include(e => e.Line)
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Equipment>> GetAllWithRelationsAsync()
    {
        return await _dbSet
            .Include(e => e.Line)
            .Include(e => e.Tags)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _dbSet.AnyAsync(e => e.Name == name);
    }

    public async Task<bool> NameExistsAsync(string name, int excludeId)
    {
        return await _dbSet.AnyAsync(e => e.Name == name && e.Id != excludeId);
    }

    public async Task<bool> HasLineAsync(int id)
    {
        var equipment = await _dbSet
            .Include(e => e.Line)
            .FirstOrDefaultAsync(e => e.Id == id);

        return equipment?.Line != null;
    }
}
