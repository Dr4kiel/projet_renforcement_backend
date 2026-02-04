namespace server.Models;

public class Role
{
    public int Id { get; set; }
    public string? Name { get; set; }

    // Navigation property
    public ICollection<User> Users { get; set; } = new List<User>();
}
