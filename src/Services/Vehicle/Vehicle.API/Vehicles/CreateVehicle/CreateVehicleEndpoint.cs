namespace Vehicle.API.Vehicles.CreateVehicle;

public record CreateVehicleRequest(string PlateNumber, VehicleSize VehicleSize);

public record CreateVehicleResponse(ParkingResponseDto Response);

public class CreateVehicleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/vehicles",
                async (CreateVehicleRequest request, ISender sender) =>
                {
                    var command = request.Adapt<CreateVehicleCommand>();

                    var result = await sender.Send(command);

                    var response = result.Adapt<CreateVehicleResponse>();

                    return Results.Created($"/vehicles/{response.Response.PlateNumber}", response);
                })
            .WithName("CreateVehicle")
            .Produces<CreateVehicleResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("CreateVehicle")
            .WithDescription("Create a new vehicle.");
    }
}