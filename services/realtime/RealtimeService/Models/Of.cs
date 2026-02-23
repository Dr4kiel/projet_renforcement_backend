namespace RealtimeService.Models;

public class Of
{
    public int Id { get; set; }
    public string Of_ { get; set; } = string.Empty;
    public string Produit { get; set; } = string.Empty;
    public int QteProduite { get; set; }
    public int QteTotale { get; set; }

    // Navigation properties
    public ICollection<Line> LinesEnCours { get; set; } = new List<Line>();
    public ICollection<Line> LinesSuivant { get; set; } = new List<Line>();
}
