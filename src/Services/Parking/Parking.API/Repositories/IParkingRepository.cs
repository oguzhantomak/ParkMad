namespace Parking.API.Repositories;

public interface IParkingRepository
{
    Task<ParkingSpot> GetAvailableSpotAsync(VehicleSize size);
    Task<ParkingSpot> GetSpotByPlateNumber(string plateNumber);
    void Update(ParkingSpot spot);

}