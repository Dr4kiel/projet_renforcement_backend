namespace server.DTOs.User;

public class UserDto
{
    public int Id { get; set; }
    public string Identifiant { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
}
