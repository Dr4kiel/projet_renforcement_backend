using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Line;

public class UpdateLineRequestDto
{
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string? Name { get; set; }

    public bool? IsChangement { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "TempsChangement must be a non-negative integer")]
    public int? TempsChangement { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "EquipmentId must be a positive integer")]
    public int? EquipmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "OfEnCoursId must be a positive integer")]
    public int? OfEnCoursId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "OfSuivantId must be a positive integer")]
    public int? OfSuivantId { get; set; }

    public bool ClearOfEnCours { get; set; }
    public bool ClearOfSuivant { get; set; }
}
