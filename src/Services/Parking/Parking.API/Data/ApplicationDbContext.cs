namespace Parking.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor correctly calling the base class constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ParkingSpot> ParkingSpots { get; set; }
        public DbSet<ParkingZone> ParkingZones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data
            modelBuilder.Entity<ParkingZone>().HasData(
                new ParkingZone { Id = 1, Name = "Zone A", Capacity = 1, VehicleSize = VehicleSize.Small },
                new ParkingZone { Id = 2, Name = "Zone B", Capacity = 2, VehicleSize = VehicleSize.Medium },
                new ParkingZone { Id = 3, Name = "Zone C", Capacity = 3, VehicleSize = VehicleSize.Large }
            );

            modelBuilder.Entity<ParkingSpot>().HasData(
                new ParkingSpot { Id = 1, ZoneId = 1, IsOccupied = false, VehicleSize = VehicleSize.Small },
                new ParkingSpot { Id = 2, ZoneId = 2, IsOccupied = false, VehicleSize = VehicleSize.Medium },
                new ParkingSpot { Id = 3, ZoneId = 2, IsOccupied = false, VehicleSize = VehicleSize.Large },
                new ParkingSpot { Id = 4, ZoneId = 3, IsOccupied = true, VehicleSize = VehicleSize.Small },
                new ParkingSpot { Id = 5, ZoneId = 3, IsOccupied = true, VehicleSize = VehicleSize.Medium },
                new ParkingSpot { Id = 6, ZoneId = 3, IsOccupied = false, VehicleSize = VehicleSize.Large }
            );
        }
    }
}