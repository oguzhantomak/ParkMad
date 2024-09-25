namespace Vehicle.API.Models;

public class Vehicle
{
    public Guid Id { get; set; }
    public string PlateNumber { get; set; } = default!;
    public VehicleSize VehicleSize { get; set; } = default!;
}