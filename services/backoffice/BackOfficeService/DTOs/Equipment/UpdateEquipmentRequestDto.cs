using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Equipment;

public class UpdateEquipmentRequestDto
{
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string? Name { get; set; }

    public List<int>? TagIds { get; set; }
}
