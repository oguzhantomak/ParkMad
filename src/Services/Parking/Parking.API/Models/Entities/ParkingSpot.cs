namespace Parking.API.Models.Entities;

public class ParkingSpot
{
    public int Id { get; set; }
    public int ZoneId { get; set; }
    public ParkingZone Zone { get; set; }
    public bool IsOccupied { get; set; }
    public VehicleSize VehicleSize { get; set; }
    public DateTime? OccupiedAt { get; set; }
}