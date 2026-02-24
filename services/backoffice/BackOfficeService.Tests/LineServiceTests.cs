using Moq;
using server.DTOs.Line;
using server.Models;
using server.Repositories.Interfaces;
using server.Services;
using FluentAssertions;

public class LineServiceTests
{
    private readonly Mock<ILineRepository> _lineRepositoryMock;
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IOfRepository> _ofRepositoryMock;
    private readonly LineService _lineService;

    public LineServiceTests()
    {
        _lineRepositoryMock = new Mock<ILineRepository>();
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _ofRepositoryMock = new Mock<IOfRepository>();
        _lineService = new LineService(
            _lineRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            _ofRepositoryMock.Object);
    }

    #region GetAllLinesAsync Tests

    [Fact]
    public async Task GetAllLinesAsync_ReturnsAllLines()
    {
        // Arrange
        var lines = new List<Line>
        {
            new Line
            {
                Id = 1,
                Name = "Line1",
                IsChangement = false,
                TempsChangement = 0,
                EquipmentId = 1,
                Equipment = new Equipment { Id = 1, Name = "Equipment1" },
                OfEnCoursId = 1,
                OfEnCours = new Of { Id = 1, Of_ = "OF001" },
                OfSuivantId = 2,
                OfSuivant = new Of { Id = 2, Of_ = "OF002" }
            },
            new Line
            {
                Id = 2,
                Name = "Line2",
                IsChangement = true,
                TempsChangement = 30,
                EquipmentId = 2,
                Equipment = new Equipment { Id = 2, Name = "Equipment2" },
                OfEnCoursId = null,
                OfEnCours = null,
                OfSuivantId = null,
                OfSuivant = null
            }
        };

        _lineRepositoryMock
            .Setup(r => r.GetAllWithRelationsAsync())
            .ReturnsAsync(lines);

        // Act
        var result = await _lineService.GetAllLinesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Line1");
        result.First().IsChangement.Should().BeFalse();
        result.First().EquipmentName.Should().Be("Equipment1");
        result.First().OfEnCoursName.Should().Be("OF001");
        result.First().OfSuivantName.Should().Be("OF002");
        result.Last().Name.Should().Be("Line2");
        result.Last().IsChangement.Should().BeTrue();
        result.Last().TempsChangement.Should().Be(30);
        result.Last().OfEnCoursId.Should().BeNull();
        result.Last().OfSuivantId.Should().BeNull();
    }

    #endregion

    #region GetLineByIdAsync Tests

    [Fact]
    public async Task GetLineByIdAsync_WhenLineExists_ReturnsLine()
    {
        // Arrange
        var line = new Line
        {
            Id = 1,
            Name = "Line1",
            IsChangement = false,
            TempsChangement = 0,
            EquipmentId = 1,
            Equipment = new Equipment { Id = 1, Name = "Equipment1" },
            OfEnCoursId = 1,
            OfEnCours = new Of { Id = 1, Of_ = "OF001" },
            OfSuivantId = null,
            OfSuivant = null
        };

        _lineRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(line);

        // Act
        var result = await _lineService.GetLineByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Line1");
        result.EquipmentName.Should().Be("Equipment1");
        result.OfEnCoursName.Should().Be("OF001");
        result.OfSuivantId.Should().BeNull();
    }

    [Fact]
    public async Task GetLineByIdAsync_WhenLineDoesNotExist_ReturnsNull()
    {
        // Arrange
        _lineRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(999))
            .ReturnsAsync((Line?)null);

        // Act
        var result = await _lineService.GetLineByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateLineAsync Tests

    [Fact]
    public async Task CreateLineAsync_WhenNameExists_ThrowsInvalidOperationException()
    {
        // Arrange
        _lineRepositoryMock
            .Setup(r => r.NameExistsAsync("Line1"))
            .ReturnsAsync(true);

        var request = new CreateLineRequestDto
        {
            Name = "Line1",
            EquipmentId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _lineService.CreateLineAsync(request));
    }

    [Fact]
    public async Task CreateLineAsync_WhenEquipmentNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _lineRepositoryMock
            .Setup(r => r.NameExistsAsync("Line1"))
            .ReturnsAsync(false);
        _equipmentRepositoryMock
            .Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        var request = new CreateLineRequestDto
        {
            Name = "Line1",
            EquipmentId = 999
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _lineService.CreateLineAsync(request));
    }

    [Fact]
    public async Task CreateLineAsync_WhenEquipmentAlreadyUsed_ThrowsInvalidOperationException()
    {
        // Arrange
        _lineRepositoryMock
            .Setup(r => r.NameExistsAsync("Line1"))
            .ReturnsAsync(false);
        _equipmentRepositoryMock
            .Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);
        _lineRepositoryMock
            .Setup(r => r.EquipmentIsUsedAsync(1))
            .ReturnsAsync(true);

        var request = new CreateLineRequestDto
        {
            Name = "Line1",
            EquipmentId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _lineService.CreateLineAsync(request));
    }

    [Fact]
    public async Task CreateLineAsync_WhenOfEnCoursNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _lineRepositoryMock
            .Setup(r => r.NameExistsAsync("Line1"))
            .ReturnsAsync(false);
        _equipmentRepositoryMock
            .Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);
        _lineRepositoryMock
            .Setup(r => r.EquipmentIsUsedAsync(1))
            .ReturnsAsync(false);
        _ofRepositoryMock
            .Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        var request = new CreateLineRequestDto
        {
            Name = "Line1",
            EquipmentId = 1,
            OfEnCoursId = 999
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _lineService.CreateLineAsync(request));
    }

    [Fact]
    public async Task CreateLineAsync_WhenOfSuivantNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _lineRepositoryMock
            .Setup(r => r.NameExistsAsync("Line1"))
            .ReturnsAsync(false);
        _equipmentRepositoryMock
            .Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);
        _lineRepositoryMock
            .Setup(r => r.EquipmentIsUsedAsync(1))
            .ReturnsAsync(false);
        _ofRepositoryMock
            .Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        var request = new CreateLineRequestDto
        {
            Name = "Line1",
            EquipmentId = 1,
            OfSuivantId = 999
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _lineService.CreateLineAsync(request));
    }

    [Fact]
    public async Task CreateLineAsync_WithValidData_CreatesLineSuccessfully()
    {
        // Arrange
        _lineRepositoryMock
            .Setup(r => r.NameExistsAsync("Line1"))
            .ReturnsAsync(false);
        _equipmentRepositoryMock
            .Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);
        _lineRepositoryMock
            .Setup(r => r.EquipmentIsUsedAsync(1))
            .ReturnsAsync(false);
        _ofRepositoryMock
            .Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);
        _ofRepositoryMock
            .Setup(r => r.ExistsAsync(2))
            .ReturnsAsync(true);

        var createdLine = new Line
        {
            Id = 1,
            Name = "Line1",
            IsChangement = true,
            TempsChangement = 30,
            EquipmentId = 1,
            Equipment = new Equipment { Id = 1, Name = "Equipment1" },
            OfEnCoursId = 1,
            OfEnCours = new Of { Id = 1, Of_ = "OF001" },
            OfSuivantId = 2,
            OfSuivant = new Of { Id = 2, Of_ = "OF002" }
        };

        _lineRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Line>()))
            .ReturnsAsync(createdLine);
        _lineRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(createdLine);

        var request = new CreateLineRequestDto
        {
            Name = "Line1",
            IsChangement = true,
            TempsChangement = 30,
            EquipmentId = 1,
            OfEnCoursId = 1,
            OfSuivantId = 2
        };

        // Act
        var result = await _lineService.CreateLineAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Line1");
        result.IsChangement.Should().BeTrue();
        result.TempsChangement.Should().Be(30);
        result.EquipmentId.Should().Be(1);
        result.OfEnCoursId.Should().Be(1);
        result.OfSuivantId.Should().Be(2);
        _lineRepositoryMock.Verify(r => r.CreateAsync(It.Is<Line>(l =>
            l.Name == "Line1" &&
            l.IsChangement == true &&
            l.TempsChangement == 30 &&
            l.EquipmentId == 1 &&
            l.OfEnCoursId == 1 &&
            l.OfSuivantId == 2
        )), Times.Once);
    }

    #endregion

    #region UpdateLineAsync Tests

    [Fact]
    public async Task UpdateLineAsync_WhenLineDoesNotExist_ReturnsNull()
    {
        // Arrange
        _lineRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Line?)null);

        var request = new UpdateLineRequestDto { Name = "NewName" };

        // Act
        var result = await _lineService.UpdateLineAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLineAsync_WhenChangingNameToExistingOne_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingLine = new Line
        {
            Id = 1,
            Name = "Line1",
            EquipmentId = 1,
            IsChangement = false,
            TempsChangement = 0
        };

        _lineRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingLine);
        _lineRepositoryMock
            .Setup(r => r.NameExistsAsync("Line2", 1))
            .ReturnsAsync(true);

        var request = new UpdateLineRequestDto { Name = "Line2" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _lineService.UpdateLineAsync(1, request));
    }

    [Fact]
    public async Task UpdateLineAsync_WhenChangingToNonExistentEquipment_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingLine = new Line
        {
            Id = 1,
            Name = "Line1",
            EquipmentId = 1,
            IsChangement = false,
            TempsChangement = 0
        };

        _lineRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingLine);
        _equipmentRepositoryMock
            .Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        var request = new UpdateLineRequestDto { EquipmentId = 999 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _lineService.UpdateLineAsync(1, request));
    }

    [Fact]
    public async Task UpdateLineAsync_WhenChangingToUsedEquipment_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingLine = new Line
        {
            Id = 1,
            Name = "Line1",
            EquipmentId = 1,
            IsChangement = false,
            TempsChangement = 0
        };

        _lineRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingLine);
        _equipmentRepositoryMock
            .Setup(r => r.ExistsAsync(2))
            .ReturnsAsync(true);
        _lineRepositoryMock
            .Setup(r => r.EquipmentIsUsedAsync(2, 1))
            .ReturnsAsync(true);

        var request = new UpdateLineRequestDto { EquipmentId = 2 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _lineService.UpdateLineAsync(1, request));
    }

    [Fact]
    public async Task UpdateLineAsync_WithClearOfEnCours_ClearsOfEnCours()
    {
        // Arrange
        var existingLine = new Line
        {
            Id = 1,
            Name = "Line1",
            EquipmentId = 1,
            OfEnCoursId = 1,
            OfSuivantId = null,
            IsChangement = false,
            TempsChangement = 0
        };

        var updatedLine = new Line
        {
            Id = 1,
            Name = "Line1",
            EquipmentId = 1,
            Equipment = new Equipment { Id = 1, Name = "Equipment1" },
            OfEnCoursId = null,
            OfEnCours = null,
            OfSuivantId = null,
            OfSuivant = null,
            IsChangement = false,
            TempsChangement = 0
        };

        _lineRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingLine);
        _lineRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Line>()))
            .ReturnsAsync(existingLine);
        _lineRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(updatedLine);

        var request = new UpdateLineRequestDto { ClearOfEnCours = true };

        // Act
        var result = await _lineService.UpdateLineAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.OfEnCoursId.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLineAsync_WithClearOfSuivant_ClearsOfSuivant()
    {
        // Arrange
        var existingLine = new Line
        {
            Id = 1,
            Name = "Line1",
            EquipmentId = 1,
            OfEnCoursId = null,
            OfSuivantId = 1,
            IsChangement = false,
            TempsChangement = 0
        };

        var updatedLine = new Line
        {
            Id = 1,
            Name = "Line1",
            EquipmentId = 1,
            Equipment = new Equipment { Id = 1, Name = "Equipment1" },
            OfEnCoursId = null,
            OfEnCours = null,
            OfSuivantId = null,
            OfSuivant = null,
            IsChangement = false,
            TempsChangement = 0
        };

        _lineRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingLine);
        _lineRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Line>()))
            .ReturnsAsync(existingLine);
        _lineRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(updatedLine);

        var request = new UpdateLineRequestDto { ClearOfSuivant = true };

        // Act
        var result = await _lineService.UpdateLineAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.OfSuivantId.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLineAsync_WithValidData_UpdatesLineSuccessfully()
    {
        // Arrange
        var existingLine = new Line
        {
            Id = 1,
            Name = "Line1",
            EquipmentId = 1,
            OfEnCoursId = null,
            OfSuivantId = null,
            IsChangement = false,
            TempsChangement = 0
        };

        var updatedLine = new Line
        {
            Id = 1,
            Name = "UpdatedLine",
            EquipmentId = 2,
            Equipment = new Equipment { Id = 2, Name = "Equipment2" },
            OfEnCoursId = 1,
            OfEnCours = new Of { Id = 1, Of_ = "OF001" },
            OfSuivantId = 2,
            OfSuivant = new Of { Id = 2, Of_ = "OF002" },
            IsChangement = true,
            TempsChangement = 45
        };

        _lineRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingLine);
        _lineRepositoryMock
            .Setup(r => r.NameExistsAsync("UpdatedLine", 1))
            .ReturnsAsync(false);
        _equipmentRepositoryMock
            .Setup(r => r.ExistsAsync(2))
            .ReturnsAsync(true);
        _lineRepositoryMock
            .Setup(r => r.EquipmentIsUsedAsync(2, 1))
            .ReturnsAsync(false);
        _ofRepositoryMock
            .Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);
        _ofRepositoryMock
            .Setup(r => r.ExistsAsync(2))
            .ReturnsAsync(true);
        _lineRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Line>()))
            .ReturnsAsync(existingLine);
        _lineRepositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1))
            .ReturnsAsync(updatedLine);

        var request = new UpdateLineRequestDto
        {
            Name = "UpdatedLine",
            EquipmentId = 2,
            OfEnCoursId = 1,
            OfSuivantId = 2,
            IsChangement = true,
            TempsChangement = 45
        };

        // Act
        var result = await _lineService.UpdateLineAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("UpdatedLine");
        result.EquipmentId.Should().Be(2);
        result.OfEnCoursId.Should().Be(1);
        result.OfSuivantId.Should().Be(2);
        result.IsChangement.Should().BeTrue();
        result.TempsChangement.Should().Be(45);
        _lineRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Line>()), Times.Once);
    }

    #endregion

    #region DeleteLineAsync Tests

    [Fact]
    public async Task DeleteLineAsync_DeletesSuccessfully()
    {
        // Arrange
        _lineRepositoryMock
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _lineService.DeleteLineAsync(1);

        // Assert
        result.Should().BeTrue();
        _lineRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteLineAsync_WhenLineDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _lineRepositoryMock
            .Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _lineService.DeleteLineAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
