using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Identifiant is required")]
    [StringLength(50, ErrorMessage = "Identifiant cannot exceed 50 characters")]
    public string Identifiant { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;
}
