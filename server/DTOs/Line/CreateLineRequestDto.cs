using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Line;

public class CreateLineRequestDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    public bool IsChangement { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "TempsChangement must be a non-negative integer")]
    public int TempsChangement { get; set; }

    [Required(ErrorMessage = "EquipmentId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "EquipmentId must be a positive integer")]
    public int EquipmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "OfEnCoursId must be a positive integer")]
    public int? OfEnCoursId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "OfSuivantId must be a positive integer")]
    public int? OfSuivantId { get; set; }
}
