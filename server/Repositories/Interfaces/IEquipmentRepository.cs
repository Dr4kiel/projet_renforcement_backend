using server.Models;

namespace server.Repositories.Interfaces;

public interface IEquipmentRepository : IGenericRepository<Equipment>
{
    Task<Equipment?> GetByIdWithRelationsAsync(int id);
    Task<IEnumerable<Equipment>> GetAllWithRelationsAsync();
    Task<bool> NameExistsAsync(string name);
    Task<bool> NameExistsAsync(string name, int excludeId);
    Task<bool> HasLineAsync(int id);
}
