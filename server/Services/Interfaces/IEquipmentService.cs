using server.DTOs.Equipment;

namespace server.Services.Interfaces;

public interface IEquipmentService
{
    Task<IEnumerable<EquipmentDto>> GetAllEquipmentsAsync();
    Task<EquipmentDto?> GetEquipmentByIdAsync(int id);
    Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentRequestDto request);
    Task<EquipmentDto?> UpdateEquipmentAsync(int id, UpdateEquipmentRequestDto request);
    Task<bool> DeleteEquipmentAsync(int id);
}
