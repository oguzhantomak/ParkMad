using System.Text.Json;
using BuildingBlocks.Common.Enums;
using BuildingBlocks.Events;
using FluentAssertions;
using Marten;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Vehicle.API.Models.DTOs;
using Vehicle.API.Vehicles.CreateVehicle;

namespace park_mad.Tests;

public class CreateVehicleCommandHandlerTests
{
    private readonly Mock<IDocumentSession> _sessionMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<CreateVehicleCommandHandler>> _loggerMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly CreateVehicleCommandHandler _handler;

    public CreateVehicleCommandHandlerTests()
    {
        _sessionMock = new Mock<IDocumentSession>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<CreateVehicleCommandHandler>>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _handler = new CreateVehicleCommandHandler(
            _sessionMock.Object,
            _cacheMock.Object,
            _loggerMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldStoreVehicleAndPublishEvent_WhenCommandIsValid()
    {
        var command = new CreateVehicleCommand("01ABC123", VehicleSize.Medium);
        var cancellationToken = CancellationToken.None;

        var vehicle = new Vehicle.API.Models.Vehicle
        {
            PlateNumber = command.PlateNumber,
            VehicleSize = command.VehicleSize
        };

        var parkingResponseDto = new ParkingResponseDto
        {
            SpotId = 1,
            AssignedAt = DateTime.UtcNow,
            ZoneName = "A",
            PlateNumber = command.PlateNumber
        };
        var cachedResponse = JsonSerializer.Serialize(parkingResponseDto);

        int callCount = 0;
        _cacheMock.Setup(x => x.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount >= 3 ? cachedResponse : null;
            });

        var result = await _handler.Handle(command, cancellationToken);

        _sessionMock.Verify(x => x.Store(It.IsAny<Vehicle.API.Models.Vehicle>()), Times.Once);
        _sessionMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);

        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<VehicleCreatedEvent>(e => e.PlateNumber == command.PlateNumber && e.VehicleSize == command.VehicleSize),
            cancellationToken), Times.Once);

        _cacheMock.Verify(x => x.GetStringAsync($"ParkingResponse_{command.PlateNumber}", It.IsAny<CancellationToken>()), Times.AtLeast(1));


        result.Should().NotBeNull();
        result.Response.PlateNumber.Should().Be(command.PlateNumber);
        result.Response.SpotId.Should().Be(parkingResponseDto.SpotId);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenParkingResponseNotRetrieved()
    {
        var command = new CreateVehicleCommand("01ABC123", VehicleSize.Medium);
        var cancellationToken = CancellationToken.None;

        _cacheMock.Setup(x => x.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        Func<Task> act = async () => await _handler.Handle(command, cancellationToken);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Parking response could not be retrieved from Redis.");

        _sessionMock.Verify(x => x.Store(It.IsAny<Vehicle.API.Models.Vehicle>()), Times.Once);
        _sessionMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);

        _publishEndpointMock.Verify(x => x.Publish(
            It.IsAny<VehicleCreatedEvent>(),
            cancellationToken), Times.Once);

        _cacheMock.Verify(x => x.GetStringAsync($"ParkingResponse_{command.PlateNumber}", It.IsAny<CancellationToken>()), Times.Exactly(5));

    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenCommandIsNull()
    {
        CreateVehicleCommand command = null;
        var cancellationToken = CancellationToken.None;

        Func<Task> act = async () => await _handler.Handle(command, cancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("command");

        _sessionMock.Verify(x => x.Store(It.IsAny<Vehicle.API.Models.Vehicle>()), Times.Never);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<VehicleCreatedEvent>(), cancellationToken), Times.Never);
    }
}