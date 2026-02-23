using server.Models;

namespace server.Repositories.Interfaces;

public interface IOfRepository : IGenericRepository<Of>
{
    Task<Of?> GetByIdWithLinesAsync(int id);
    Task<IEnumerable<Of>> GetAllWithLinesCountAsync();
    Task<bool> OfNameExistsAsync(string ofName);
    Task<bool> OfNameExistsAsync(string ofName, int excludeId);
    Task<bool> HasLinesAsync(int id);
}
