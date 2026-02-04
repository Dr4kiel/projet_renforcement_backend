using server.Models;

namespace server.Repositories.Interfaces;

public interface ITagRepository : IGenericRepository<Tag>
{
    Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids);
}
