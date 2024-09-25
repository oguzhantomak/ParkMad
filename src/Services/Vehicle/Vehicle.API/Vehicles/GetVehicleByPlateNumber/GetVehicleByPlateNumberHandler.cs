namespace Vehicle.API.Vehicles.GetVehicleByPlateNumber;

public record GetVehicleByPlateNumberQuery(string PlateNumber) : IQuery<GetVehicleByPlateNumberResult>;
public record GetVehicleByPlateNumberResult(Models.Vehicle Vehicle);

public class GetVehicleByPlateNumberQueryHandler(IDocumentSession session) : IQueryHandler<GetVehicleByPlateNumberQuery, GetVehicleByPlateNumberResult>
{
    public async Task<GetVehicleByPlateNumberResult> Handle(GetVehicleByPlateNumberQuery query, CancellationToken cancellationToken)
    {
        var vehicle = await session.Query<Models.Vehicle>().FirstOrDefaultAsync(v => v.PlateNumber == query.PlateNumber, cancellationToken);

        return new GetVehicleByPlateNumberResult(vehicle);
    }
}