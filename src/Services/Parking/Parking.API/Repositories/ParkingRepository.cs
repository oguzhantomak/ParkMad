namespace Parking.API.Repositories;

public class ParkingRepository(ApplicationDbContext context) : IParkingRepository
{
    public async Task<ParkingSpot> GetAvailableSpotAsync(VehicleSize size)
    {
        var count = await context.ParkingSpots.CountAsync();

        return await context.ParkingSpots
            .Include(ps => ps.Zone)
            .Where(ps => !ps.IsOccupied && ps.VehicleSize == size)
            .FirstOrDefaultAsync();
    }

    public void Update(ParkingSpot spot)
    {
        context.ParkingSpots.Update(spot);
    }
}