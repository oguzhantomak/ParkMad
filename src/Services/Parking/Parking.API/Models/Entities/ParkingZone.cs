namespace Parking.API.Models.Entities;

public class ParkingZone
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }
    public VehicleSize VehicleSize { get; set; }
    public ICollection<ParkingSpot> ParkingSpots { get; set; }
}