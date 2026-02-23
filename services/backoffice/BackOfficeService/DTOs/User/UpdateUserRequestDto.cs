using System.ComponentModel.DataAnnotations;

namespace server.DTOs.User;

public class UpdateUserRequestDto
{
    [StringLength(50, ErrorMessage = "Identifiant cannot exceed 50 characters")]
    public string? Identifiant { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "RoleId must be a positive integer")]
    public int? RoleId { get; set; }
}
