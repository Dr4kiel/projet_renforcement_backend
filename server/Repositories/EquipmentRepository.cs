using server.Data;
using server.Models;
using server.Repositories.Interfaces;

namespace server.Repositories;

public class EquipmentRepository : GenericRepository<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(ApplicationDbContext context) : base(context) { }
}
