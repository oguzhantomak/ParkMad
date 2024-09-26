namespace Parking.API.Services
{
    public class ParkingService : IParkingService, IConsumer<VehicleCreatedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ParkingService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public ParkingService(
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILogger<ParkingService> logger,
            IPublishEndpoint publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<VehicleCreatedEvent> context)
        {
            var vehicleCreatedEvent = context.Message;
            _logger.LogInformation($"Received VehicleCreatedEvent: PlateNumber = {vehicleCreatedEvent.PlateNumber}, VehicleSize = {vehicleCreatedEvent.VehicleSize}");

            _cache.Set("VehicleCreatedEventData", vehicleCreatedEvent, TimeSpan.FromMinutes(2));

            try
            {
                var spot = await _unitOfWork.ParkingRepository.GetAvailableSpotAsync(vehicleCreatedEvent.VehicleSize);

                if (spot == null)
                {
                    // TODO: Rollback event will be triggered!
                    _logger.LogWarning("No suitable parking spot found.");
                    throw new Exception("No suitable parking spot found.");
                }

                spot.IsOccupied = true;
                spot.OccupiedAt = DateTime.UtcNow;
                _unitOfWork.ParkingRepository.Update(spot);

                await _unitOfWork.CompleteAsync();

                var parkingResponse = new ParkingResponseDto
                {
                    SpotId = spot.Id,
                    AssignedAt = DateTime.UtcNow,
                    ZoneName = spot.Zone.Name
                };

                await _publishEndpoint.Publish(parkingResponse);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
            // Removed redundant await Task.CompletedTask;
        }

        public async Task<ParkingResponseDto> AssignParkingSpotAsync(ParkingRequestDto request)
        {
            try
            {
                var spot = await _unitOfWork.ParkingRepository.GetAvailableSpotAsync(VehicleSize.Large);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            throw new NotImplementedException();
        }
    }
}
