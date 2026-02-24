using Moq;
using server.DTOs.Equipment;
using server.Models;
using server.Repositories.Interfaces;
using server.Services;
using FluentAssertions;

public class EquipmentServiceTests
{
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly EquipmentService _equipmentService;

    public EquipmentServiceTests()
    {
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _tagRepositoryMock = new Mock<ITagRepository>();
        _equipmentService = new EquipmentService(_equipmentRepositoryMock.Object, _tagRepositoryMock.Object);
    }

    #region GetAllEquipmentsAsync Tests

    [Fact]
    public async Task GetAllEquipmentsAsync_ReturnsAllEquipments()
    {
        // Arrange
        var equipments = new List<Equipment>
        {
            new Equipment
            {
                Id = 1,
                Name = "Equipment1",
                Line = new Line { Id = 1, Name = "Line1" },
                Tags = new List<Tag>
                {
                    new Tag { Id = 1, TagName = "Tag1" },
                    new Tag { Id = 2, TagName = "Tag2" }
                }
            },
            new Equipment
            {
                Id = 2,
                Name = "Equipment2",
                Line = null,
                Tags = new List<Tag>()
            }
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetAllWithRelationsAsync())
            .ReturnsAsync(equipments);

        // Act
        var result = await _equipmentService.GetAllEquipmentsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Equipment1");
        result.First().LineId.Should().Be(1);
        result.First().LineName.Should().Be("Line1");
        result.First().TagsCount.Should().Be(2);
        result.First().Tags.Should().HaveCount(2);
        result.Last().Name.Should().Be("Equipment2");
        result.Last().LineId.Should().BeNull();
        result.Last().TagsCount.Should().Be(0);
    }

    #endregion

    #region GetEquipmentByIdAsync Tests

    [Fact]
    public async Task GetEquipmentByIdAsync_WhenEquipmentExists_ReturnsEquipment()
    {
        // Arrange
        var equipment = new Equipment
        {
            Id = 1,
            Name = "Equipment1",
            Line = new Line { Id = 1, Name = "Line1" },
            Tags = new List<Tag>
            {
                new Tag { Id = 1, TagName = "Tag1" }
            }
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(equipment);

        // Act
        var result = await _equipmentService.GetEquipmentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Equipment1");
        result.LineId.Should().Be(1);
        result.LineName.Should().Be("Line1");
        result.TagsCount.Should().Be(1);
    }

    [Fact]
    public async Task GetEquipmentByIdAsync_WhenEquipmentDoesNotExist_ReturnsNull()
    {
        // Arrange
        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(999))
            .ReturnsAsync((Equipment?)null);

        // Act
        var result = await _equipmentService.GetEquipmentByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateEquipmentAsync Tests

    [Fact]
    public async Task CreateEquipmentAsync_WhenNameExists_ThrowsInvalidOperationException()
    {
        // Arrange
        _equipmentRepositoryMock
            .Setup(r => r.NameExistsAsync("Equipment1"))
            .ReturnsAsync(true);

        var request = new CreateEquipmentRequestDto { Name = "Equipment1" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _equipmentService.CreateEquipmentAsync(request));
    }

    [Fact]
    public async Task CreateEquipmentAsync_WhenTagsNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _equipmentRepositoryMock
            .Setup(r => r.NameExistsAsync("Equipment1"))
            .ReturnsAsync(false);

        var tagIds = new List<int> { 1, 2, 3 };
        var foundTags = new List<Tag>
        {
            new Tag { Id = 1, TagName = "Tag1" },
            new Tag { Id = 2, TagName = "Tag2" }
        };

        _tagRepositoryMock
            .Setup(r => r.GetByIdsAsync(tagIds))
            .ReturnsAsync(foundTags);

        var request = new CreateEquipmentRequestDto
        {
            Name = "Equipment1",
            TagIds = tagIds
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _equipmentService.CreateEquipmentAsync(request));
    }

    [Fact]
    public async Task CreateEquipmentAsync_WithValidDataAndTags_CreatesEquipmentSuccessfully()
    {
        // Arrange
        _equipmentRepositoryMock
            .Setup(r => r.NameExistsAsync("Equipment1"))
            .ReturnsAsync(false);

        var tagIds = new List<int> { 1, 2 };
        var tags = new List<Tag>
        {
            new Tag { Id = 1, TagName = "Tag1" },
            new Tag { Id = 2, TagName = "Tag2" }
        };

        _tagRepositoryMock
            .Setup(r => r.GetByIdsAsync(tagIds))
            .ReturnsAsync(tags);

        var createdEquipment = new Equipment
        {
            Id = 1,
            Name = "Equipment1",
            Tags = tags
        };

        _equipmentRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Equipment>()))
            .ReturnsAsync(createdEquipment);
        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(createdEquipment);

        var request = new CreateEquipmentRequestDto
        {
            Name = "Equipment1",
            TagIds = tagIds
        };

        // Act
        var result = await _equipmentService.CreateEquipmentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Equipment1");
        result.TagsCount.Should().Be(2);
        _equipmentRepositoryMock.Verify(r => r.CreateAsync(It.Is<Equipment>(e =>
            e.Name == "Equipment1" &&
            e.Tags.Count == 2
        )), Times.Once);
    }

    [Fact]
    public async Task CreateEquipmentAsync_WithoutTags_CreatesEquipmentSuccessfully()
    {
        // Arrange
        _equipmentRepositoryMock
            .Setup(r => r.NameExistsAsync("Equipment1"))
            .ReturnsAsync(false);

        var createdEquipment = new Equipment
        {
            Id = 1,
            Name = "Equipment1",
            Tags = new List<Tag>()
        };

        _equipmentRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Equipment>()))
            .ReturnsAsync(createdEquipment);
        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(createdEquipment);

        var request = new CreateEquipmentRequestDto { Name = "Equipment1" };

        // Act
        var result = await _equipmentService.CreateEquipmentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Equipment1");
        result.TagsCount.Should().Be(0);
    }

    #endregion

    #region UpdateEquipmentAsync Tests

    [Fact]
    public async Task UpdateEquipmentAsync_WhenEquipmentDoesNotExist_ReturnsNull()
    {
        // Arrange
        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(999))
            .ReturnsAsync((Equipment?)null);

        var request = new UpdateEquipmentRequestDto { Name = "NewName" };

        // Act
        var result = await _equipmentService.UpdateEquipmentAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateEquipmentAsync_WhenChangingNameToExistingOne_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingEquipment = new Equipment
        {
            Id = 1,
            Name = "Equipment1",
            Tags = new List<Tag>()
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(existingEquipment);
        _equipmentRepositoryMock
            .Setup(r => r.NameExistsAsync("Equipment2", 1))
            .ReturnsAsync(true);

        var request = new UpdateEquipmentRequestDto { Name = "Equipment2" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _equipmentService.UpdateEquipmentAsync(1, request));
    }

    [Fact]
    public async Task UpdateEquipmentAsync_WhenTagsNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingEquipment = new Equipment
        {
            Id = 1,
            Name = "Equipment1",
            Tags = new List<Tag>()
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(existingEquipment);

        var tagIds = new List<int> { 1, 2, 3 };
        var foundTags = new List<Tag>
        {
            new Tag { Id = 1, TagName = "Tag1" }
        };

        _tagRepositoryMock
            .Setup(r => r.GetByIdsAsync(tagIds))
            .ReturnsAsync(foundTags);

        var request = new UpdateEquipmentRequestDto
        {
            Name = "Equipment1",
            TagIds = tagIds
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _equipmentService.UpdateEquipmentAsync(1, request));
    }

    [Fact]
    public async Task UpdateEquipmentAsync_WithValidData_UpdatesEquipmentSuccessfully()
    {
        // Arrange
        var existingEquipment = new Equipment
        {
            Id = 1,
            Name = "Equipment1",
            Tags = new List<Tag>()
        };

        var updatedEquipment = new Equipment
        {
            Id = 1,
            Name = "UpdatedEquipment",
            Tags = new List<Tag>
            {
                new Tag { Id = 1, TagName = "Tag1" }
            }
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(existingEquipment);
        _equipmentRepositoryMock
            .Setup(r => r.NameExistsAsync("UpdatedEquipment", 1))
            .ReturnsAsync(false);

        var tagIds = new List<int> { 1 };
        var tags = new List<Tag> { new Tag { Id = 1, TagName = "Tag1" } };
        _tagRepositoryMock
            .Setup(r => r.GetByIdsAsync(tagIds))
            .ReturnsAsync(tags);

        _equipmentRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Equipment>()))
            .ReturnsAsync(existingEquipment);
        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(updatedEquipment);

        var request = new UpdateEquipmentRequestDto
        {
            Name = "UpdatedEquipment",
            TagIds = tagIds
        };

        // Act
        var result = await _equipmentService.UpdateEquipmentAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("UpdatedEquipment");
        result.TagsCount.Should().Be(1);
        _equipmentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Equipment>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEquipmentAsync_WithEmptyTagIds_ClearsTags()
    {
        // Arrange
        var existingEquipment = new Equipment
        {
            Id = 1,
            Name = "Equipment1",
            Tags = new List<Tag>
            {
                new Tag { Id = 1, TagName = "Tag1" }
            }
        };

        var updatedEquipment = new Equipment
        {
            Id = 1,
            Name = "Equipment1",
            Tags = new List<Tag>()
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(existingEquipment);
        _equipmentRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Equipment>()))
            .ReturnsAsync(existingEquipment);
        _equipmentRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(updatedEquipment);

        var request = new UpdateEquipmentRequestDto
        {
            Name = "Equipment1",
            TagIds = new List<int>()
        };

        // Act
        var result = await _equipmentService.UpdateEquipmentAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.TagsCount.Should().Be(0);
    }

    #endregion

    #region DeleteEquipmentAsync Tests

    [Fact]
    public async Task DeleteEquipmentAsync_WhenEquipmentHasLine_ThrowsInvalidOperationException()
    {
        // Arrange
        _equipmentRepositoryMock
            .Setup(r => r.HasLineAsync(1))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _equipmentService.DeleteEquipmentAsync(1));
        _equipmentRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteEquipmentAsync_WhenEquipmentHasNoLine_DeletesSuccessfully()
    {
        // Arrange
        _equipmentRepositoryMock
            .Setup(r => r.HasLineAsync(1))
            .ReturnsAsync(false);
        _equipmentRepositoryMock
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _equipmentService.DeleteEquipmentAsync(1);

        // Assert
        result.Should().BeTrue();
        _equipmentRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteEquipmentAsync_WhenEquipmentDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _equipmentRepositoryMock
            .Setup(r => r.HasLineAsync(999))
            .ReturnsAsync(false);
        _equipmentRepositoryMock
            .Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _equipmentService.DeleteEquipmentAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
