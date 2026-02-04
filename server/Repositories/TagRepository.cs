using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Repositories.Interfaces;

namespace server.Repositories;

public class TagRepository : GenericRepository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _dbSet
            .Where(t => ids.Contains(t.Id))
            .ToListAsync();
    }

    public async Task<Tag?> GetByIdWithRelationsAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Equipments)
            .Include(t => t.Historians)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Tag>> GetAllWithRelationsAsync()
    {
        return await _dbSet
            .Include(t => t.Equipments)
            .Include(t => t.Historians)
            .ToListAsync();
    }

    public async Task<bool> TagNameExistsAsync(string tagName)
    {
        return await _dbSet.AnyAsync(t => t.TagName == tagName);
    }

    public async Task<bool> TagNameExistsAsync(string tagName, int excludeId)
    {
        return await _dbSet.AnyAsync(t => t.TagName == tagName && t.Id != excludeId);
    }

    public async Task<bool> HasEquipmentsAsync(int id)
    {
        var tag = await _dbSet
            .Include(t => t.Equipments)
            .FirstOrDefaultAsync(t => t.Id == id);

        return tag?.Equipments?.Any() ?? false;
    }

    public async Task<bool> HasHistoriansAsync(int id)
    {
        var tag = await _dbSet
            .Include(t => t.Historians)
            .FirstOrDefaultAsync(t => t.Id == id);

        return tag?.Historians?.Any() ?? false;
    }
}
