using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Of;

public class UpdateOfRequestDto
{
    [StringLength(100, ErrorMessage = "OF cannot exceed 100 characters")]
    public string? Of { get; set; }

    [StringLength(200, ErrorMessage = "Produit cannot exceed 200 characters")]
    public string? Produit { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "QteProduite must be a non-negative integer")]
    public int? QteProduite { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "QteTotale must be a positive integer")]
    public int? QteTotale { get; set; }
}
