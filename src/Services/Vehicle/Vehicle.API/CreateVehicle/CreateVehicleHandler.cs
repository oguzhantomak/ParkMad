namespace Vehicle.API.CreateVehicle;

public record CreateVehicleCommand(string LicensePlate, VehicleSize VehicleSize) : IRequest<CreateVehicleResult>;

/// <summary>
/// Geriye oluşturulan aracın Id'sini döner.
/// </summary>
/// <param name="Id"></param>
public record CreateVehicleResult(Guid Id);

internal class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, CreateVehicleResult>
{
    public async Task<CreateVehicleResult> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}