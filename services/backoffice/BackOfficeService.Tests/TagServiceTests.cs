using Moq;
using server.DTOs.Tag;
using server.Models;
using server.Repositories.Interfaces;
using server.Services;
using FluentAssertions;

public class TagServiceTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly TagService _tagService;

    public TagServiceTests()
    {
        _tagRepositoryMock = new Mock<ITagRepository>();
        _tagService = new TagService(_tagRepositoryMock.Object);
    }

    #region GetAllTagsAsync Tests

    [Fact]
    public async Task GetAllTagsAsync_ReturnsAllTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            new Tag
            {
                Id = 1,
                TagName = "Temperature",
                Equipments = new List<Equipment>
                {
                    new Equipment { Id = 1, Name = "Sensor1" },
                    new Equipment { Id = 2, Name = "Sensor2" }
                },
                Historians = new List<Historian>
                {
                    new Historian { Id = 1, Timestamp = DateTime.UtcNow, Value = 25.5m }
                }
            },
            new Tag
            {
                Id = 2,
                TagName = "Pressure",
                Equipments = new List<Equipment>(),
                Historians = new List<Historian>()
            }
        };

        _tagRepositoryMock
            .Setup(r => r.GetAllWithRelationsAsync())
            .ReturnsAsync(tags);

        // Act
        var result = await _tagService.GetAllTagsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().TagName.Should().Be("Temperature");
        result.First().EquipmentsCount.Should().Be(2);
        result.First().HistoriansCount.Should().Be(1);
        result.Last().TagName.Should().Be("Pressure");
        result.Last().EquipmentsCount.Should().Be(0);
        result.Last().HistoriansCount.Should().Be(0);
    }

    #endregion

    #region GetTagByIdAsync Tests

    [Fact]
    public async Task GetTagByIdAsync_WhenTagExists_ReturnsTag()
    {
        // Arrange
        var tag = new Tag
        {
            Id = 1,
            TagName = "Temperature",
            Equipments = new List<Equipment>
            {
                new Equipment { Id = 1, Name = "Sensor1" }
            },
            Historians = new List<Historian>()
        };

        _tagRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(tag);

        // Act
        var result = await _tagService.GetTagByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.TagName.Should().Be("Temperature");
        result.EquipmentsCount.Should().Be(1);
        result.HistoriansCount.Should().Be(0);
    }

    [Fact]
    public async Task GetTagByIdAsync_WhenTagDoesNotExist_ReturnsNull()
    {
        // Arrange
        _tagRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(999))
            .ReturnsAsync((Tag?)null);

        // Act
        var result = await _tagService.GetTagByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateTagAsync Tests

    [Fact]
    public async Task CreateTagAsync_WhenTagNameExists_ThrowsInvalidOperationException()
    {
        // Arrange
        _tagRepositoryMock
            .Setup(r => r.TagNameExistsAsync("Temperature"))
            .ReturnsAsync(true);

        var request = new CreateTagRequestDto { TagName = "Temperature" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _tagService.CreateTagAsync(request));
    }

    [Fact]
    public async Task CreateTagAsync_WithValidData_CreatesTagSuccessfully()
    {
        // Arrange
        _tagRepositoryMock
            .Setup(r => r.TagNameExistsAsync("NewTag"))
            .ReturnsAsync(false);

        var createdTag = new Tag
        {
            Id = 1,
            TagName = "NewTag",
            Equipments = new List<Equipment>(),
            Historians = new List<Historian>()
        };

        _tagRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Tag>()))
            .ReturnsAsync(createdTag);
        _tagRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(createdTag);

        var request = new CreateTagRequestDto { TagName = "NewTag" };

        // Act
        var result = await _tagService.CreateTagAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TagName.Should().Be("NewTag");
        result.EquipmentsCount.Should().Be(0);
        result.HistoriansCount.Should().Be(0);
        _tagRepositoryMock.Verify(r => r.CreateAsync(It.Is<Tag>(t =>
            t.TagName == "NewTag"
        )), Times.Once);
    }

    #endregion

    #region UpdateTagAsync Tests

    [Fact]
    public async Task UpdateTagAsync_WhenTagDoesNotExist_ReturnsNull()
    {
        // Arrange
        _tagRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Tag?)null);

        var request = new UpdateTagRequestDto { TagName = "NewName" };

        // Act
        var result = await _tagService.UpdateTagAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTagAsync_WhenChangingNameToExistingOne_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingTag = new Tag
        {
            Id = 1,
            TagName = "Temperature",
            Equipments = new List<Equipment>(),
            Historians = new List<Historian>()
        };

        _tagRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingTag);
        _tagRepositoryMock
            .Setup(r => r.TagNameExistsAsync("Pressure", 1))
            .ReturnsAsync(true);

        var request = new UpdateTagRequestDto { TagName = "Pressure" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _tagService.UpdateTagAsync(1, request));
    }

    [Fact]
    public async Task UpdateTagAsync_WithSameName_SkipsUniquenessCheck()
    {
        // Arrange
        var existingTag = new Tag
        {
            Id = 1,
            TagName = "Temperature",
            Equipments = new List<Equipment>(),
            Historians = new List<Historian>()
        };

        _tagRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingTag);
        _tagRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Tag>()))
            .ReturnsAsync(existingTag);
        _tagRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(existingTag);

        var request = new UpdateTagRequestDto { TagName = "Temperature" };

        // Act
        var result = await _tagService.UpdateTagAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        _tagRepositoryMock.Verify(r => r.TagNameExistsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTagAsync_WithValidData_UpdatesTagSuccessfully()
    {
        // Arrange
        var existingTag = new Tag
        {
            Id = 1,
            TagName = "Temperature",
            Equipments = new List<Equipment>(),
            Historians = new List<Historian>()
        };

        var updatedTag = new Tag
        {
            Id = 1,
            TagName = "NewTemperature",
            Equipments = new List<Equipment>
            {
                new Equipment { Id = 1, Name = "Sensor1" }
            },
            Historians = new List<Historian>()
        };

        _tagRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingTag);
        _tagRepositoryMock
            .Setup(r => r.TagNameExistsAsync("NewTemperature", 1))
            .ReturnsAsync(false);
        _tagRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Tag>()))
            .ReturnsAsync(existingTag);
        _tagRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(updatedTag);

        var request = new UpdateTagRequestDto { TagName = "NewTemperature" };

        // Act
        var result = await _tagService.UpdateTagAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.TagName.Should().Be("NewTemperature");
        result.EquipmentsCount.Should().Be(1);
        _tagRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Tag>()), Times.Once);
    }

    #endregion

    #region DeleteTagAsync Tests

    [Fact]
    public async Task DeleteTagAsync_WhenTagHasHistorians_ThrowsInvalidOperationException()
    {
        // Arrange
        _tagRepositoryMock
            .Setup(r => r.HasHistoriansAsync(1))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _tagService.DeleteTagAsync(1));
        _tagRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTagAsync_WhenTagHasNoHistorians_DeletesSuccessfully()
    {
        // Arrange
        _tagRepositoryMock
            .Setup(r => r.HasHistoriansAsync(1))
            .ReturnsAsync(false);
        _tagRepositoryMock
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _tagService.DeleteTagAsync(1);

        // Assert
        result.Should().BeTrue();
        _tagRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteTagAsync_WhenTagDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _tagRepositoryMock
            .Setup(r => r.HasHistoriansAsync(999))
            .ReturnsAsync(false);
        _tagRepositoryMock
            .Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _tagService.DeleteTagAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
