using server.Models;

namespace server.Repositories.Interfaces;

public interface ILineRepository : IGenericRepository<Line>
{
    Task<Line?> GetByIdWithRelationsAsync(int id);
    Task<IEnumerable<Line>> GetAllWithRelationsAsync();
    Task<bool> NameExistsAsync(string name);
    Task<bool> NameExistsAsync(string name, int excludeId);
    Task<bool> EquipmentIsUsedAsync(int equipmentId);
    Task<bool> EquipmentIsUsedAsync(int equipmentId, int excludeLineId);
}
