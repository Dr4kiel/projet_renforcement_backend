using server.Data;
using server.Models;
using server.Repositories.Interfaces;

namespace server.Repositories;

public class OfRepository : GenericRepository<Of>, IOfRepository
{
    public OfRepository(ApplicationDbContext context) : base(context) { }
}
