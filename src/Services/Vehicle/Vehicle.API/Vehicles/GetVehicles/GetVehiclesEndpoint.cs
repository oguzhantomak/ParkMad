namespace Vehicle.API.Vehicles.GetVehicles;

public record GetVehiclesRequest(int? PageNumber = 1, int? PageSize = 10);
public record GetVehiclesResponse(IEnumerable<Models.Vehicle> Vehicles);
public class GetVehiclesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/vehicles",
                async ([AsParameters] GetVehiclesRequest request, ISender sender) =>
                {
                    var result = await sender.Send(new GetVehiclesQuery());

                    var response = result.Adapt<GetVehiclesResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetVehicles")
            .Produces<GetVehiclesResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("GetVehicles")
            .WithDescription("Get all vehicles.");
    }
}