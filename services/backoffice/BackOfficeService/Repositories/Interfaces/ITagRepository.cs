using server.Models;

namespace server.Repositories.Interfaces;

public interface ITagRepository : IGenericRepository<Tag>
{
    Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids);
    Task<Tag?> GetByIdWithRelationsAsync(int id);
    Task<IEnumerable<Tag>> GetAllWithRelationsAsync();
    Task<bool> TagNameExistsAsync(string tagName);
    Task<bool> TagNameExistsAsync(string tagName, int excludeId);
    Task<bool> HasEquipmentsAsync(int id);
    Task<bool> HasHistoriansAsync(int id);
}
