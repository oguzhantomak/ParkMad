namespace Vehicle.API.Vehicles.CreateVehicle;

public record CreateVehicleCommand(string PlateNumber, VehicleSize VehicleSize) : ICommand<CreateVehicleResult>;
public record CreateVehicleResult(ParkingResponseDto Response);

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

        var plateRegex = new Regex(@"^[0-8][0-9]{1}[A-Z]{3}[0-9]{3}$");
        return plateRegex.IsMatch(plateNumber);
    }
}

public class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand, CreateVehicleResult>
{
    private readonly IDocumentSession _session;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CreateVehicleCommandHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateVehicleCommandHandler(IDocumentSession session, IDistributedCache distributedCache, ILogger<CreateVehicleCommandHandler> logger, IPublishEndpoint publishEndpoint)
    {
        _session = session;
        _distributedCache = distributedCache;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateVehicleResult> Handle(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        if (command is not null)
        {
            var vehicle = new Models.Vehicle
            {
                PlateNumber = command.PlateNumber,
                VehicleSize = command.VehicleSize
            };

            _session.Store(vehicle);
            await _session.SaveChangesAsync(cancellationToken);

            var vehicleCreatedEvent = new VehicleCreatedEvent(vehicle.PlateNumber, vehicle.VehicleSize);
            await _publishEndpoint.Publish(vehicleCreatedEvent, cancellationToken);

            var policy = Policy.HandleResult<string>(result => result == null)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(2), (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"Retrying to get ParkingResponse from Redis... Attempt: {retryCount}");
                });

            string cachedResponse = await policy.ExecuteAsync(async () =>
            {
                return await _distributedCache.GetStringAsync($"ParkingResponse_{vehicle.PlateNumber}");
            });

            if (!string.IsNullOrEmpty(cachedResponse))
            {
                var parkingResponse = JsonSerializer.Deserialize<ParkingResponseDto>(cachedResponse);

                if (parkingResponse != null)
                {
                    _logger.LogInformation($"Successfully retrieved ParkingResponse from Redis: SpotId = {parkingResponse.SpotId}");

                    return new CreateVehicleResult(parkingResponse);
                }
            }

            _logger.LogError("Failed to retrieve ParkingResponse from Redis after 5 attempts.");
            throw new Exception("Parking response could not be retrieved from Redis.");
        }

        throw new ArgumentNullException(nameof(command));
    }
}
