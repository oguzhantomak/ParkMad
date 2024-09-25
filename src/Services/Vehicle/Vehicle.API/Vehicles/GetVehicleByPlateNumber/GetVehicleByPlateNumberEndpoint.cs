namespace Vehicle.API.Vehicles.GetVehicleByPlateNumber;

public record GetVehicleByPlateNumberRequest(string PlateNumber);
public record GetVehicleByPlateNumberResponse(Models.Vehicle Vehicle);

public class GetVehicleByPlateNumberEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/vehicles/{PlateNumber}",
                async ([AsParameters] GetVehicleByPlateNumberRequest request, ISender sender) =>
                {
                    var result = await sender.Send(new GetVehicleByPlateNumberQuery(request.PlateNumber));

                    var response = result.Adapt<GetVehicleByPlateNumberResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetVehicleByPlateNumber")
            .Produces<GetVehicleByPlateNumberResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("GetVehicleByPlateNumber")
            .WithDescription("Get a vehicle by plate number.");
    }
}