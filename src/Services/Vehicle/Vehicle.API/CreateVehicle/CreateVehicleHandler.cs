namespace Vehicle.API.CreateVehicle;

public record CreateVehicleCommand(string LicensePlate, VehicleSize VehicleSize) : ICommand<CreateVehicleResult>;

/// <summary>
/// Geriye oluşturulan aracın Id'sini döner.
/// </summary>
/// <param name="Id"></param>
public record CreateVehicleResult(Guid Id);

internal class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand, CreateVehicleResult>
{
    public async Task<CreateVehicleResult> Handle(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        //TODO: Create vehicle business logic

        if (command is not null)
        {
            var vehicle = new Models.Vehicle
            {
                LicensePlate = command.LicensePlate,
                VehicleSize = command.VehicleSize
            };
        }

        //TODO: Save vehicle to database
        //TODO: Return vehicle Id

        //TODO: Temp, will be deleted
        return new CreateVehicleResult(Guid.NewGuid());
    }
}