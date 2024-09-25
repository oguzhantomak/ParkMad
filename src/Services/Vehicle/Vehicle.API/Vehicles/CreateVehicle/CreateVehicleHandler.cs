namespace Vehicle.API.Vehicles.CreateVehicle;

public record CreateVehicleCommand(string PlateNumber, VehicleSize VehicleSize) : ICommand<CreateVehicleResult>;

/// <summary>
/// Geriye oluşturulan aracın Id'sini, park yeri ID'sini, bölge adını ve atama zamanını döner.
/// </summary>
public record CreateVehicleResult(Guid Id, int SpotId, string ZoneName, DateTime AssignedAt);

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

internal class CreateVehicleCommandHandler : ICommandHandler<CreateVehicleCommand, CreateVehicleResult>, IConsumer<ParkingResponseDto>
{
    private readonly IDocumentSession _session;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CreateVehicleCommandHandler> _logger;

    public CreateVehicleCommandHandler(IDocumentSession session, IPublishEndpoint publishEndpoint, IMemoryCache cache, ILogger<CreateVehicleCommandHandler> logger)
    {
        _session = session;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ParkingResponseDto> context)
    {
        var parkingResponse = context.Message;

        _logger.LogInformation($"Received ParkingResponse: SpotId = {parkingResponse.SpotId}, AssignedAt = {parkingResponse.AssignedAt}, ZoneName = {parkingResponse.ZoneName}");

        _cache.Set(parkingResponse.SpotId, parkingResponse, TimeSpan.FromMinutes(5));

        await Task.CompletedTask;
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


            var policy = Policy.HandleResult<ParkingResponseDto>(result => result == null)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(2), (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"Retrying to get ParkingResponse from cache... Attempt: {retryCount}");
                });

            ParkingResponseDto parkingResponse = await policy.ExecuteAsync(() =>
            {
                _cache.TryGetValue(vehicle.PlateNumber, out ParkingResponseDto cachedResponse);
                return Task.FromResult(cachedResponse);
            });

            if (parkingResponse != null)
            {
                _logger.LogInformation($"Successfully retrieved ParkingResponse from cache: SpotId = {parkingResponse.SpotId}");

                return new CreateVehicleResult(vehicle.Id, parkingResponse.SpotId, parkingResponse.ZoneName, parkingResponse.AssignedAt);
            }
            else
            {
                _logger.LogError("Failed to retrieve ParkingResponse from cache after 5 attempts.");
                throw new Exception("Parking response could not be retrieved from cache.");
            }
        }

        throw new ArgumentNullException(nameof(command));
    }
}
