

namespace Vehicle.API.Vehicles.CreateVehicle;

public record CreateVehicleCommand(string PlateNumber, VehicleSize VehicleSize) : ICommand<CreateVehicleResult>;

/// <summary>
/// Geriye oluşturulan aracın Id'sini döner.
/// </summary>
/// <param name="Id"></param>
public record CreateVehicleResult(Guid Id);

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.PlateNumber)
            .NotEmpty().WithMessage("Plate number is required.")
            .Must(plateNumber => IsValidPlateNumber(plateNumber))
            .WithMessage("Invalid plate number format. Expected format: 01AAA123");
        RuleFor(x => x.VehicleSize).IsInEnum().WithMessage("Vehicle size is invalid.");
    }

    private bool IsValidPlateNumber(string plateNumber)
    {
        if (string.IsNullOrEmpty(plateNumber))
            return false;

        var plateRegex = new Regex(@"^[0-8][0-9][A-Z]{3}[0-9]{3}$");
        return plateRegex.IsMatch(plateNumber);
    }
}

internal class CreateVehicleCommandHandler(IDocumentSession session, IPublishEndpoint publishEndpoint) : ICommandHandler<CreateVehicleCommand, CreateVehicleResult>
{
    public async Task<CreateVehicleResult> Handle(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        if (command is not null)
        {
            var vehicle = new Models.Vehicle
            {
                PlateNumber = command.PlateNumber,
                VehicleSize = command.VehicleSize
            };

            session.Store(vehicle);
            await session.SaveChangesAsync(cancellationToken);
            
            var vehicleCreatedEvent = new VehicleCreatedEvent(vehicle.PlateNumber, vehicle.VehicleSize);
            await publishEndpoint.Publish(vehicleCreatedEvent, cancellationToken);
            
            return new CreateVehicleResult(vehicle.Id);
        }

        throw new ArgumentNullException(nameof(command));
    }
}