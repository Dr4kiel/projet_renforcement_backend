namespace server.DTOs.Tag;

public class TagDto
{
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;
    public int EquipmentsCount { get; set; }
    public int HistoriansCount { get; set; }
}
