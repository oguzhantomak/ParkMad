namespace Parking.API.UnitOfWork;

public interface IUnitOfWork
{
    IParkingRepository ParkingRepository { get; }
    Task<int> CompleteAsync();
    void Rollback();
}