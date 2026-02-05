using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using server.Repositories.Interfaces;

namespace server.Repositories;

public class LineRepository : GenericRepository<Line>, ILineRepository
{
    public LineRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Line?> GetByIdWithRelationsAsync(int id)
    {
        return await _dbSet
            .Include(l => l.Equipment)
            .Include(l => l.OfEnCours)
            .Include(l => l.OfSuivant)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Line>> GetAllWithRelationsAsync()
    {
        return await _dbSet
            .Include(l => l.Equipment)
            .Include(l => l.OfEnCours)
            .Include(l => l.OfSuivant)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _dbSet.AnyAsync(l => l.Name == name);
    }

    public async Task<bool> NameExistsAsync(string name, int excludeId)
    {
        return await _dbSet.AnyAsync(l => l.Name == name && l.Id != excludeId);
    }

    public async Task<bool> EquipmentIsUsedAsync(int equipmentId)
    {
        return await _dbSet.AnyAsync(l => l.EquipmentId == equipmentId);
    }

    public async Task<bool> EquipmentIsUsedAsync(int equipmentId, int excludeLineId)
    {
        return await _dbSet.AnyAsync(l => l.EquipmentId == equipmentId && l.Id != excludeLineId);
    }
}
