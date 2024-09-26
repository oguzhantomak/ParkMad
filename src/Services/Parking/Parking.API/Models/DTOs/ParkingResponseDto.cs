namespace Parking.API.Models.DTOs;

public class ParkingResponseDto
{
    public int SpotId { get; set; }
    public string ZoneName { get; set; }
    public DateTime AssignedAt { get; set; }
    public string PlateNumber { get; set; }
}