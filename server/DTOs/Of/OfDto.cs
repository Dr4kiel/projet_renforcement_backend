namespace server.DTOs.Of;

public class OfDto
{
    public int Id { get; set; }
    public string Of { get; set; } = string.Empty;
    public string Produit { get; set; } = string.Empty;
    public int QteProduite { get; set; }
    public int QteTotale { get; set; }
    public int LinesEnCoursCount { get; set; }
    public int LinesSuivantCount { get; set; }
}
