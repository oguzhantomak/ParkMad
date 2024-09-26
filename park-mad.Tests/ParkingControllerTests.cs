using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Parking.API.Controllers;
using Parking.API.Models.DTOs;
using Parking.API.Services;

namespace park_mad.Tests;

public class ParkingControllerTests
{
    private readonly Mock<IParkingService> _parkingServiceMock;
    private readonly ParkingController _controller;

    public ParkingControllerTests()
    {
        _parkingServiceMock = new Mock<IParkingService>();
        _controller = new ParkingController(_parkingServiceMock.Object);
    }

    [Fact]
    public async Task ReleaseParkingSpot_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var request = new UnparkRequestDto
        {
            PlateNumber = "34ABC123"
        };

        var expectedResponse = new UnparkResponseDto
        {
            Price = new decimal(50.0),
            TotalHours = 2.5
        };

        _parkingServiceMock.Setup(s => s.ReleaseParkingSpotAsync(It.IsAny<UnparkRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ReleaseParkingSpot(request);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task ReleaseParkingSpot_ShouldReturnError_WhenParkingSpotNotFound()
    {
        // Arrange
        var request = new UnparkRequestDto
        {
            PlateNumber = "34XYZ456"
        };

        _parkingServiceMock.Setup(s => s.ReleaseParkingSpotAsync(It.IsAny<UnparkRequestDto>()))
            .ThrowsAsync(new Exception("Parking spot not found."));

        // Act
        Func<Task> act = async () => await _controller.ReleaseParkingSpot(request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Parking spot not found.");
    }
}