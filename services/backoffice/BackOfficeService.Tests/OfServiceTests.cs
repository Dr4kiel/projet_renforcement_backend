using Moq;
using server.DTOs.Of;
using server.Models;
using server.Repositories.Interfaces;
using server.Services;
using FluentAssertions;

public class OfServiceTests
{
    private readonly Mock<IOfRepository> _ofRepositoryMock;
    private readonly OfService _ofService;

    public OfServiceTests()
    {
        _ofRepositoryMock = new Mock<IOfRepository>();
        _ofService = new OfService(_ofRepositoryMock.Object);
    }

    #region GetAllOfsAsync Tests

    [Fact]
    public async Task GetAllOfsAsync_ReturnsAllOfs()
    {
        // Arrange
        var ofs = new List<Of>
        {
            new Of
            {
                Id = 1,
                Of_ = "OF001",
                Produit = "Product1",
                QteProduite = 50,
                QteTotale = 100,
                LinesEnCours = new List<Line>
                {
                    new Line { Id = 1, Name = "Line1" }
                },
                LinesSuivant = new List<Line>
                {
                    new Line { Id = 2, Name = "Line2" },
                    new Line { Id = 3, Name = "Line3" }
                }
            },
            new Of
            {
                Id = 2,
                Of_ = "OF002",
                Produit = "Product2",
                QteProduite = 0,
                QteTotale = 200,
                LinesEnCours = new List<Line>(),
                LinesSuivant = new List<Line>()
            }
        };

        _ofRepositoryMock
            .Setup(r => r.GetAllWithLinesCountAsync())
            .ReturnsAsync(ofs);

        // Act
        var result = await _ofService.GetAllOfsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Of.Should().Be("OF001");
        result.First().QteProduite.Should().Be(50);
        result.First().QteTotale.Should().Be(100);
        result.First().LinesEnCoursCount.Should().Be(1);
        result.First().LinesSuivantCount.Should().Be(2);
        result.Last().Of.Should().Be("OF002");
        result.Last().LinesEnCoursCount.Should().Be(0);
        result.Last().LinesSuivantCount.Should().Be(0);
    }

    #endregion

    #region GetOfByIdAsync Tests

    [Fact]
    public async Task GetOfByIdAsync_WhenOfExists_ReturnsOf()
    {
        // Arrange
        var of = new Of
        {
            Id = 1,
            Of_ = "OF001",
            Produit = "Product1",
            QteProduite = 50,
            QteTotale = 100,
            LinesEnCours = new List<Line>
            {
                new Line { Id = 1, Name = "Line1" }
            },
            LinesSuivant = new List<Line>()
        };

        _ofRepositoryMock
            .Setup(r => r.GetByIdWithLinesAsync(1))
            .ReturnsAsync(of);

        // Act
        var result = await _ofService.GetOfByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Of.Should().Be("OF001");
        result.Produit.Should().Be("Product1");
        result.LinesEnCoursCount.Should().Be(1);
        result.LinesSuivantCount.Should().Be(0);
    }

    [Fact]
    public async Task GetOfByIdAsync_WhenOfDoesNotExist_ReturnsNull()
    {
        // Arrange
        _ofRepositoryMock
            .Setup(r => r.GetByIdWithLinesAsync(999))
            .ReturnsAsync((Of?)null);

        // Act
        var result = await _ofService.GetOfByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateOfAsync Tests

    [Fact]
    public async Task CreateOfAsync_WhenOfNameExists_ThrowsInvalidOperationException()
    {
        // Arrange
        _ofRepositoryMock
            .Setup(r => r.OfNameExistsAsync("OF001"))
            .ReturnsAsync(true);

        var request = new CreateOfRequestDto
        {
            Of = "OF001",
            Produit = "Product1",
            QteProduite = 0,
            QteTotale = 100
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _ofService.CreateOfAsync(request));
    }

    [Fact]
    public async Task CreateOfAsync_WhenQteProduiteExceedsQteTotale_ThrowsInvalidOperationException()
    {
        // Arrange
        _ofRepositoryMock
            .Setup(r => r.OfNameExistsAsync("OF001"))
            .ReturnsAsync(false);

        var request = new CreateOfRequestDto
        {
            Of = "OF001",
            Produit = "Product1",
            QteProduite = 150,
            QteTotale = 100
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _ofService.CreateOfAsync(request));
    }

    [Fact]
    public async Task CreateOfAsync_WithValidData_CreatesOfSuccessfully()
    {
        // Arrange
        _ofRepositoryMock
            .Setup(r => r.OfNameExistsAsync("OF001"))
            .ReturnsAsync(false);

        var createdOf = new Of
        {
            Id = 1,
            Of_ = "OF001",
            Produit = "Product1",
            QteProduite = 50,
            QteTotale = 100,
            LinesEnCours = new List<Line>(),
            LinesSuivant = new List<Line>()
        };

        _ofRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Of>()))
            .ReturnsAsync(createdOf);
        _ofRepositoryMock
            .Setup(r => r.GetByIdWithLinesAsync(1))
            .ReturnsAsync(createdOf);

        var request = new CreateOfRequestDto
        {
            Of = "OF001",
            Produit = "Product1",
            QteProduite = 50,
            QteTotale = 100
        };

        // Act
        var result = await _ofService.CreateOfAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Of.Should().Be("OF001");
        result.Produit.Should().Be("Product1");
        result.QteProduite.Should().Be(50);
        result.QteTotale.Should().Be(100);
        _ofRepositoryMock.Verify(r => r.CreateAsync(It.Is<Of>(o =>
            o.Of_ == "OF001" &&
            o.Produit == "Product1" &&
            o.QteProduite == 50 &&
            o.QteTotale == 100
        )), Times.Once);
    }

    #endregion

    #region UpdateOfAsync Tests

    [Fact]
    public async Task UpdateOfAsync_WhenOfDoesNotExist_ReturnsNull()
    {
        // Arrange
        _ofRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Of?)null);

        var request = new UpdateOfRequestDto { Of = "NewName" };

        // Act
        var result = await _ofService.UpdateOfAsync(999, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateOfAsync_WhenChangingNameToExistingOne_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingOf = new Of
        {
            Id = 1,
            Of_ = "OF001",
            Produit = "Product1",
            QteProduite = 0,
            QteTotale = 100
        };

        _ofRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingOf);
        _ofRepositoryMock
            .Setup(r => r.OfNameExistsAsync("OF002", 1))
            .ReturnsAsync(true);

        var request = new UpdateOfRequestDto { Of = "OF002" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _ofService.UpdateOfAsync(1, request));
    }

    [Fact]
    public async Task UpdateOfAsync_WhenQteProduiteExceedsQteTotale_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingOf = new Of
        {
            Id = 1,
            Of_ = "OF001",
            Produit = "Product1",
            QteProduite = 50,
            QteTotale = 100
        };

        _ofRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingOf);

        var request = new UpdateOfRequestDto
        {
            QteProduite = 150
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _ofService.UpdateOfAsync(1, request));
    }

    [Fact]
    public async Task UpdateOfAsync_WithValidData_UpdatesOfSuccessfully()
    {
        // Arrange
        var existingOf = new Of
        {
            Id = 1,
            Of_ = "OF001",
            Produit = "Product1",
            QteProduite = 50,
            QteTotale = 100
        };

        var updatedOf = new Of
        {
            Id = 1,
            Of_ = "OF001-Updated",
            Produit = "Product1-Updated",
            QteProduite = 75,
            QteTotale = 150,
            LinesEnCours = new List<Line>(),
            LinesSuivant = new List<Line>()
        };

        _ofRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingOf);
        _ofRepositoryMock
            .Setup(r => r.OfNameExistsAsync("OF001-Updated", 1))
            .ReturnsAsync(false);
        _ofRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Of>()))
            .ReturnsAsync(existingOf);
        _ofRepositoryMock
            .Setup(r => r.GetByIdWithLinesAsync(1))
            .ReturnsAsync(updatedOf);

        var request = new UpdateOfRequestDto
        {
            Of = "OF001-Updated",
            Produit = "Product1-Updated",
            QteProduite = 75,
            QteTotale = 150
        };

        // Act
        var result = await _ofService.UpdateOfAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Of.Should().Be("OF001-Updated");
        result.Produit.Should().Be("Product1-Updated");
        result.QteProduite.Should().Be(75);
        result.QteTotale.Should().Be(150);
        _ofRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Of>()), Times.Once);
    }

    #endregion

    #region DeleteOfAsync Tests

    [Fact]
    public async Task DeleteOfAsync_WhenOfHasLines_ThrowsInvalidOperationException()
    {
        // Arrange
        _ofRepositoryMock
            .Setup(r => r.HasLinesAsync(1))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _ofService.DeleteOfAsync(1));
        _ofRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteOfAsync_WhenOfHasNoLines_DeletesSuccessfully()
    {
        // Arrange
        _ofRepositoryMock
            .Setup(r => r.HasLinesAsync(1))
            .ReturnsAsync(false);
        _ofRepositoryMock
            .Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _ofService.DeleteOfAsync(1);

        // Assert
        result.Should().BeTrue();
        _ofRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteOfAsync_WhenOfDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _ofRepositoryMock
            .Setup(r => r.HasLinesAsync(999))
            .ReturnsAsync(false);
        _ofRepositoryMock
            .Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _ofService.DeleteOfAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
