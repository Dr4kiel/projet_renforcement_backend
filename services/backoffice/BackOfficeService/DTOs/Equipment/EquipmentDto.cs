namespace server.DTOs.Equipment;

public class EquipmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? LineId { get; set; }
    public string? LineName { get; set; }
    public int TagsCount { get; set; }
    public List<TagInfoDto> Tags { get; set; } = new();
}

public class TagInfoDto
{
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;
}
