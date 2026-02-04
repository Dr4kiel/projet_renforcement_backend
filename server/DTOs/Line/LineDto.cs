namespace server.DTOs.Line;

public class LineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsChangement { get; set; }
    public int TempsChangement { get; set; }
    public int EquipmentId { get; set; }
    public string? EquipmentName { get; set; }
    public int? OfEnCoursId { get; set; }
    public string? OfEnCoursName { get; set; }
    public int? OfSuivantId { get; set; }
    public string? OfSuivantName { get; set; }
}
