namespace RealtimeService.Models;

public class Tag
{
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Historian> Historians { get; set; } = new List<Historian>();
    public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
}
