using server.DTOs.Tag;

namespace server.Services.Interfaces;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllTagsAsync();
    Task<TagDto?> GetTagByIdAsync(int id);
    Task<TagDto> CreateTagAsync(CreateTagRequestDto request);
    Task<TagDto?> UpdateTagAsync(int id, UpdateTagRequestDto request);
    Task<bool> DeleteTagAsync(int id);
}
