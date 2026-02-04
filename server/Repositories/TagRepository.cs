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
}
