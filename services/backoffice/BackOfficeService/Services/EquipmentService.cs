using server.DTOs.Equipment;
using server.Models;
using server.Repositories.Interfaces;
using server.Services.Interfaces;

namespace server.Services;

public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly ITagRepository _tagRepository;

    public EquipmentService(IEquipmentRepository equipmentRepository, ITagRepository tagRepository)
    {
        _equipmentRepository = equipmentRepository;
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<EquipmentDto>> GetAllEquipmentsAsync()
    {
        var equipments = await _equipmentRepository.GetAllWithRelationsAsync();
        return equipments.Select(MapToDto);
    }

    public async Task<EquipmentDto?> GetEquipmentByIdAsync(int id)
    {
        var equipment = await _equipmentRepository.GetByIdWithRelationsAsync(id);
        return equipment == null ? null : MapToDto(equipment);
    }

    public async Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentRequestDto request)
    {
        // Validate name uniqueness
        if (await _equipmentRepository.NameExistsAsync(request.Name))
        {
            throw new InvalidOperationException("Equipment name already exists");
        }

        var equipment = new Equipment
        {
            Name = request.Name
        };

        // Handle tags if provided
        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            var tags = await _tagRepository.GetByIdsAsync(request.TagIds);
            if (tags.Count() != request.TagIds.Count)
            {
                throw new InvalidOperationException("One or more tags not found");
            }

            equipment.Tags = tags.ToList();
        }

        var createdEquipment = await _equipmentRepository.CreateAsync(equipment);
        var equipmentWithRelations = await _equipmentRepository.GetByIdWithRelationsAsync(createdEquipment.Id);
        return MapToDto(equipmentWithRelations!);
    }

    public async Task<EquipmentDto?> UpdateEquipmentAsync(int id, UpdateEquipmentRequestDto request)
    {
        var equipment = await _equipmentRepository.GetByIdWithRelationsAsync(id);
        if (equipment == null)
        {
            return null;
        }

        // Validate name uniqueness if changed
        if (request.Name != null && request.Name != equipment.Name)
        {
            if (await _equipmentRepository.NameExistsAsync(request.Name, id))
            {
                throw new InvalidOperationException("Equipment name already exists");
            }

            equipment.Name = request.Name;
        }

        // Handle tags if provided
        if (request.TagIds != null)
        {
            if (request.TagIds.Count > 0)
            {
                var tags = await _tagRepository.GetByIdsAsync(request.TagIds);
                if (tags.Count() != request.TagIds.Count)
                {
                    throw new InvalidOperationException("One or more tags not found");
                }

                equipment.Tags.Clear();
                foreach (var tag in tags)
                {
                    equipment.Tags.Add(tag);
                }
            }
            else
            {
                equipment.Tags.Clear();
            }
        }

        await _equipmentRepository.UpdateAsync(equipment);
        var updatedEquipment = await _equipmentRepository.GetByIdWithRelationsAsync(id);
        return MapToDto(updatedEquipment!);
    }

    public async Task<bool> DeleteEquipmentAsync(int id)
    {
        // Check if equipment has an associated line
        if (await _equipmentRepository.HasLineAsync(id))
        {
            throw new InvalidOperationException("Cannot delete equipment that has an associated line");
        }

        return await _equipmentRepository.DeleteAsync(id);
    }

    private static EquipmentDto MapToDto(Equipment equipment)
    {
        return new EquipmentDto
        {
            Id = equipment.Id,
            Name = equipment.Name,
            LineId = equipment.Line?.Id,
            LineName = equipment.Line?.Name,
            TagsCount = equipment.Tags?.Count ?? 0,
            Tags = equipment.Tags?.Select(t => new TagInfoDto
            {
                Id = t.Id,
                TagName = t.TagName
            }).ToList() ?? new List<TagInfoDto>()
        };
    }
}
