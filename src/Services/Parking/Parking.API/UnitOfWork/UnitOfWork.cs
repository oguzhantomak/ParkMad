namespace Parking.API.UnitOfWork;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private IParkingRepository _parkingRepository;

    public IParkingRepository ParkingRepository
    {
        get { return _parkingRepository ??= new ParkingRepository(context); }
    }

    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Rollback()
    {
        context.ChangeTracker.Entries()
            .ToList()
            .ForEach(e => e.State = EntityState.Unchanged);
    }
}