namespace BuildingBlocks.Events;

public record VehicleCreatedEvent(string PlateNumber, VehicleSize VehicleSize);