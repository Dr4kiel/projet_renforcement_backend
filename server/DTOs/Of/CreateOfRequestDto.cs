using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Of;

public class CreateOfRequestDto
{
    [Required(ErrorMessage = "OF is required")]
    [StringLength(100, ErrorMessage = "OF cannot exceed 100 characters")]
    public string Of { get; set; } = string.Empty;

    [Required(ErrorMessage = "Produit is required")]
    [StringLength(200, ErrorMessage = "Produit cannot exceed 200 characters")]
    public string Produit { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "QteProduite must be a non-negative integer")]
    public int QteProduite { get; set; }

    [Required(ErrorMessage = "QteTotale is required")]
    [Range(1, int.MaxValue, ErrorMessage = "QteTotale must be a positive integer")]
    public int QteTotale { get; set; }
}
