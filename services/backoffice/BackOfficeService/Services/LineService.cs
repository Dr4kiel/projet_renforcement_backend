using server.DTOs.Line;
using server.Models;
using server.Repositories.Interfaces;
using server.Services.Interfaces;

namespace server.Services;

public class LineService : ILineService
{
    private readonly ILineRepository _lineRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IOfRepository _ofRepository;

    public LineService(
        ILineRepository lineRepository,
        IEquipmentRepository equipmentRepository,
        IOfRepository ofRepository)
    {
        _lineRepository = lineRepository;
        _equipmentRepository = equipmentRepository;
        _ofRepository = ofRepository;
    }

    public async Task<IEnumerable<LineDto>> GetAllLinesAsync()
    {
        var lines = await _lineRepository.GetAllWithRelationsAsync();
        return lines.Select(MapToDto);
    }

    public async Task<LineDto?> GetLineByIdAsync(int id)
    {
        var line = await _lineRepository.GetByIdWithRelationsAsync(id);
        return line == null ? null : MapToDto(line);
    }

    public async Task<LineDto> CreateLineAsync(CreateLineRequestDto request)
    {
        // Validate name uniqueness
        if (await _lineRepository.NameExistsAsync(request.Name))
            throw new InvalidOperationException("Line name already exists");

        // Validate equipment exists
        if (!await _equipmentRepository.ExistsAsync(request.EquipmentId))
            throw new InvalidOperationException("Equipment not found");

        // Validate equipment is not already used by another line (1:1 relationship)
        if (await _lineRepository.EquipmentIsUsedAsync(request.EquipmentId))
            throw new InvalidOperationException("Equipment is already assigned to another line");

        // Validate OF en cours if provided
        if (request.OfEnCoursId.HasValue)
        {
            if (!await _ofRepository.ExistsAsync(request.OfEnCoursId.Value))
                throw new InvalidOperationException("OF en cours not found");
        }

        // Validate OF suivant if provided
        if (request.OfSuivantId.HasValue)
        {
            if (!await _ofRepository.ExistsAsync(request.OfSuivantId.Value))
                throw new InvalidOperationException("OF suivant not found");
        }

        var line = new Line
        {
            Name = request.Name,
            IsChangement = request.IsChangement,
            TempsChangement = request.TempsChangement,
            EquipmentId = request.EquipmentId,
            OfEnCoursId = request.OfEnCoursId,
            OfSuivantId = request.OfSuivantId
        };

        var createdLine = await _lineRepository.CreateAsync(line);
        var lineWithRelations = await _lineRepository.GetByIdWithRelationsAsync(createdLine.Id);
        return MapToDto(lineWithRelations!);
    }

    public async Task<LineDto?> UpdateLineAsync(int id, UpdateLineRequestDto request)
    {
        var line = await _lineRepository.GetByIdAsync(id);
        if (line == null) return null;

        // Validate name uniqueness if changed
        if (request.Name != null && request.Name != line.Name)
        {
            if (await _lineRepository.NameExistsAsync(request.Name, id))
                throw new InvalidOperationException("Line name already exists");
            line.Name = request.Name;
        }

        // Validate equipment if changed
        if (request.EquipmentId.HasValue && request.EquipmentId.Value != line.EquipmentId)
        {
            if (!await _equipmentRepository.ExistsAsync(request.EquipmentId.Value))
                throw new InvalidOperationException("Equipment not found");

            if (await _lineRepository.EquipmentIsUsedAsync(request.EquipmentId.Value, id))
                throw new InvalidOperationException("Equipment is already assigned to another line");

            line.EquipmentId = request.EquipmentId.Value;
        }

        // Handle OF en cours
        if (request.ClearOfEnCours)
        {
            line.OfEnCoursId = null;
        }
        else if (request.OfEnCoursId.HasValue)
        {
            if (!await _ofRepository.ExistsAsync(request.OfEnCoursId.Value))
                throw new InvalidOperationException("OF en cours not found");
            line.OfEnCoursId = request.OfEnCoursId;
        }

        // Handle OF suivant
        if (request.ClearOfSuivant)
        {
            line.OfSuivantId = null;
        }
        else if (request.OfSuivantId.HasValue)
        {
            if (!await _ofRepository.ExistsAsync(request.OfSuivantId.Value))
                throw new InvalidOperationException("OF suivant not found");
            line.OfSuivantId = request.OfSuivantId;
        }

        // Update other fields if provided
        if (request.IsChangement.HasValue)
            line.IsChangement = request.IsChangement.Value;

        if (request.TempsChangement.HasValue)
            line.TempsChangement = request.TempsChangement.Value;

        await _lineRepository.UpdateAsync(line);
        var updatedLine = await _lineRepository.GetByIdWithRelationsAsync(id);
        return MapToDto(updatedLine!);
    }

    public async Task<bool> DeleteLineAsync(int id)
    {
        return await _lineRepository.DeleteAsync(id);
    }

    private static LineDto MapToDto(Line line)
    {
        return new LineDto
        {
            Id = line.Id,
            Name = line.Name,
            IsChangement = line.IsChangement,
            TempsChangement = line.TempsChangement,
            EquipmentId = line.EquipmentId,
            EquipmentName = line.Equipment?.Name,
            OfEnCoursId = line.OfEnCoursId,
            OfEnCoursName = line.OfEnCours?.Of_,
            OfSuivantId = line.OfSuivantId,
            OfSuivantName = line.OfSuivant?.Of_
        };
    }
}
