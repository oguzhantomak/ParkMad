namespace Vehicle.API.GetVehicles;

public record GetVehiclesResponse(IEnumerable<Models.Vehicle> Vehicles);
public class GetVehiclesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/vehicles",
                async (ISender sender) =>
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