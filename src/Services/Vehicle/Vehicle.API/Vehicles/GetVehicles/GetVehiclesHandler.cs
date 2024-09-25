namespace Vehicle.API.Vehicles.GetVehicles;

public record GetVehiclesQuery(int? PageNumber = 1, int? PageSize = 10) : IQuery<GetVehiclesResult>
{
    public int? PageSize { get; init; } = PageSize > 100 ? 100 : PageSize;
}
public record GetVehiclesResult(IEnumerable<Models.Vehicle> Vehicles);

public class GetVehiclesQueryHandler(IDocumentSession session) : IQueryHandler<GetVehiclesQuery, GetVehiclesResult>
{
    public async Task<GetVehiclesResult> Handle(GetVehiclesQuery query, CancellationToken cancellationToken)
    {
        var vehicles = await session.Query<Models.Vehicle>().ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);

        return new GetVehiclesResult(vehicles);
    }
}