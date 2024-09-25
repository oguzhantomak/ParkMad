namespace Vehicle.API.Models;

public class Vehicle
{
    public Guid Id { get; set; }
    public string LicensePlate { get; set; } = default!;
    public VehicleSize VehicleSize { get; set; } = default!;
}