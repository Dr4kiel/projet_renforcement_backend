using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Tag;

public class CreateTagRequestDto
{
    [Required(ErrorMessage = "TagName is required")]
    [StringLength(100, ErrorMessage = "TagName cannot exceed 100 characters")]
    public string TagName { get; set; } = string.Empty;
}
