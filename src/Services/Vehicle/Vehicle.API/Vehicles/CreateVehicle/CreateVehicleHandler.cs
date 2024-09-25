using FluentValidation;

namespace Vehicle.API.Vehicles.CreateVehicle;

public record CreateVehicleCommand(string LicensePlate, VehicleSize VehicleSize) : ICommand<CreateVehicleResult>;

/// <summary>
/// Geriye oluşturulan aracın Id'sini döner.
/// </summary>
/// <param name="Id"></param>
public record CreateVehicleResult(Guid Id);

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.LicensePlate).NotEmpty().WithMessage("License plate is required.");
        RuleFor(x => x.VehicleSize).IsInEnum().WithMessage("Vehicle size is invalid.");
    }

}

internal class CreateVehicleCommandHandler(IDocumentSession session) : ICommandHandler<CreateVehicleCommand, CreateVehicleResult>
{
    public async Task<CreateVehicleResult> Handle(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        if (command is not null)
        {
            var vehicle = new Models.Vehicle
            {
                PlateNumber = command.LicensePlate,
                VehicleSize = command.VehicleSize
            };

            session.Store(vehicle);
            await session.SaveChangesAsync(cancellationToken);

            return new CreateVehicleResult(vehicle.Id);
        }

        throw new ArgumentNullException(nameof(command));
    }
}