using server.DTOs.Of;

namespace server.Services.Interfaces;

public interface IOfService
{
    Task<IEnumerable<OfDto>> GetAllOfsAsync();
    Task<OfDto?> GetOfByIdAsync(int id);
    Task<OfDto> CreateOfAsync(CreateOfRequestDto request);
    Task<OfDto?> UpdateOfAsync(int id, UpdateOfRequestDto request);
    Task<bool> DeleteOfAsync(int id);
}
