using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Repositories.Interfaces;

namespace server.Repositories;

public class OfRepository : GenericRepository<Of>, IOfRepository
{
    public OfRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Of?> GetByIdWithLinesAsync(int id)
    {
        return await _dbSet
            .Include(o => o.LinesEnCours)
            .Include(o => o.LinesSuivant)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Of>> GetAllWithLinesCountAsync()
    {
        return await _dbSet
            .Include(o => o.LinesEnCours)
            .Include(o => o.LinesSuivant)
            .ToListAsync();
    }

    public async Task<bool> OfNameExistsAsync(string ofName)
    {
        return await _dbSet.AnyAsync(o => o.Of_ == ofName);
    }

    public async Task<bool> OfNameExistsAsync(string ofName, int excludeId)
    {
        return await _dbSet.AnyAsync(o => o.Of_ == ofName && o.Id != excludeId);
    }

    public async Task<bool> HasLinesAsync(int id)
    {
        var of = await _dbSet
            .Include(o => o.LinesEnCours)
            .Include(o => o.LinesSuivant)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (of == null) return false;
        return of.LinesEnCours.Any() || of.LinesSuivant.Any();
    }
}
