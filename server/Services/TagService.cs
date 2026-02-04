using server.DTOs.Tag;
using server.Models;
using server.Repositories.Interfaces;
using server.Services.Interfaces;

namespace server.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
    {
        var tags = await _tagRepository.GetAllWithRelationsAsync();
        return tags.Select(MapToDto);
    }

    public async Task<TagDto?> GetTagByIdAsync(int id)
    {
        var tag = await _tagRepository.GetByIdWithRelationsAsync(id);
        return tag == null ? null : MapToDto(tag);
    }

    public async Task<TagDto> CreateTagAsync(CreateTagRequestDto request)
    {
        // Validate tag name uniqueness
        if (await _tagRepository.TagNameExistsAsync(request.TagName))
            throw new InvalidOperationException("Tag name already exists");

        var tag = new Tag
        {
            TagName = request.TagName
        };

        var createdTag = await _tagRepository.CreateAsync(tag);
        var tagWithRelations = await _tagRepository.GetByIdWithRelationsAsync(createdTag.Id);
        return MapToDto(tagWithRelations!);
    }

    public async Task<TagDto?> UpdateTagAsync(int id, UpdateTagRequestDto request)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null) return null;

        // Validate tag name uniqueness if changed
        if (request.TagName != null && request.TagName != tag.TagName)
        {
            if (await _tagRepository.TagNameExistsAsync(request.TagName, id))
                throw new InvalidOperationException("Tag name already exists");
            tag.TagName = request.TagName;
        }

        await _tagRepository.UpdateAsync(tag);
        var updatedTag = await _tagRepository.GetByIdWithRelationsAsync(id);
        return MapToDto(updatedTag!);
    }

    public async Task<bool> DeleteTagAsync(int id)
    {
        // Check if tag has associated historians
        if (await _tagRepository.HasHistoriansAsync(id))
            throw new InvalidOperationException("Cannot delete tag that has associated historian data");

        return await _tagRepository.DeleteAsync(id);
    }

    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            TagName = tag.TagName,
            EquipmentsCount = tag.Equipments?.Count ?? 0,
            HistoriansCount = tag.Historians?.Count ?? 0
        };
    }
}
