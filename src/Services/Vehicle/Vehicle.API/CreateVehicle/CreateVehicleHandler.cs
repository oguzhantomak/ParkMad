namespace Vehicle.API.CreateVehicle;

public record CreateVehicleCommand(string LicensePlate, VehicleSize VehicleSize) : ICommand<CreateVehicleResult>;

/// <summary>
/// Geriye oluşturulan aracın Id'sini döner.
/// </summary>
/// <param name="Id"></param>
public record CreateVehicleResult(Guid Id);

internal class CreateVehicleCommandHandler(IDocumentSession session) : ICommandHandler<CreateVehicleCommand, CreateVehicleResult>
{
    public async Task<CreateVehicleResult> Handle(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        if (command is not null)
        {
            var vehicle = new Models.Vehicle
            {
                LicensePlate = command.LicensePlate,
                VehicleSize = command.VehicleSize
            };

            session.Store(vehicle);
            await session.SaveChangesAsync(cancellationToken);

            return new CreateVehicleResult(vehicle.Id);
        }

        throw new ArgumentNullException(nameof(command));
    }
}