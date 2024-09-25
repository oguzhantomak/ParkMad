namespace Parking.API.Repositories;

public interface IParkingRepository
{
    Task<ParkingSpot> GetAvailableSpotAsync(VehicleSize size);
    void Update(ParkingSpot spot);

}