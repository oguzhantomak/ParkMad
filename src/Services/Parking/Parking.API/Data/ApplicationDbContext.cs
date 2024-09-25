namespace Parking.API.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ParkingSpot> ParkingSpots { get; set; }
    public DbSet<ParkingZone> ParkingZones { get; set; }
}