using System.ComponentModel.DataAnnotations;

namespace server.DTOs.User;

public class CreateUserRequestDto
{
    [Required(ErrorMessage = "Identifiant is required")]
    [StringLength(50, ErrorMessage = "Identifiant cannot exceed 50 characters")]
    public string Identifiant { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "RoleId must be a positive integer")]
    public int? RoleId { get; set; }
}
