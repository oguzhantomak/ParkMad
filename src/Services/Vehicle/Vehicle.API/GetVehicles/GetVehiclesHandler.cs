namespace Vehicle.API.GetVehicles;

public record GetVehiclesQuery() : IQuery<GetVehiclesResult>;
public record GetVehiclesResult(IEnumerable<Models.Vehicle> Vehicles);

public class GetVehiclesQueryHandler(IDocumentSession session, ILogger<GetVehiclesQueryHandler> logger) : IQueryHandler<GetVehiclesQuery, GetVehiclesResult>
{
    public async Task<GetVehiclesResult> Handle(GetVehiclesQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("GetVehiclesQueryHandler.Handle called with {@Query}", query);

        var vehicles = await session.Query<Models.Vehicle>().ToListAsync(cancellationToken);

        return new GetVehiclesResult(vehicles);
    }
}