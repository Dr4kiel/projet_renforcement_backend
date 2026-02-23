using server.DTOs.Line;

namespace server.Services.Interfaces;

public interface ILineService
{
    Task<IEnumerable<LineDto>> GetAllLinesAsync();
    Task<LineDto?> GetLineByIdAsync(int id);
    Task<LineDto> CreateLineAsync(CreateLineRequestDto request);
    Task<LineDto?> UpdateLineAsync(int id, UpdateLineRequestDto request);
    Task<bool> DeleteLineAsync(int id);
}
