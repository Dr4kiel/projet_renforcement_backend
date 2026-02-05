namespace server.Models;

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public Line? Line { get; set; }
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
