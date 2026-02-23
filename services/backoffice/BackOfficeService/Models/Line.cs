namespace server.Models;

public class Line
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsChangement { get; set; }
    public int TempsChangement { get; set; }

    public int EquipmentId { get; set; }
    public int? OfSuivantId { get; set; }
    public int? OfEnCoursId { get; set; }

    // Navigation properties
    public Equipment Equipment { get; set; } = null!;
    public Of? OfSuivant { get; set; }
    public Of? OfEnCours { get; set; }
}
