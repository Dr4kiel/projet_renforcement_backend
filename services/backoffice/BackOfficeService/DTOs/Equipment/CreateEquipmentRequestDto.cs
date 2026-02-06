using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Equipment;

public class CreateEquipmentRequestDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    public List<int>? TagIds { get; set; }
}
