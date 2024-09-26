namespace Parking.API.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IParkingRepository _parkingRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IParkingRepository ParkingRepository
        {
            get { return _parkingRepository ??= new ParkingRepository(_context); }
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Rollback()
        {
            _context.ChangeTracker.Entries()
                .ToList()
                .ForEach(e => e.State = EntityState.Unchanged);
        }
    }
}