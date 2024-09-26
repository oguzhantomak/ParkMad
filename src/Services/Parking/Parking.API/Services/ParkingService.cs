namespace Parking.API.Services
{
    public class ParkingService : IParkingService, IConsumer<VehicleCreatedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<ParkingService> _logger;

        public ParkingService(
            IUnitOfWork unitOfWork,
            IDistributedCache distributedCache,
            ILogger<ParkingService> logger)
        {
            _unitOfWork = unitOfWork;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<VehicleCreatedEvent> context)
        {
            var vehicleCreatedEvent = context.Message;
            _logger.LogInformation($"Received VehicleCreatedEvent: PlateNumber = {vehicleCreatedEvent.PlateNumber}, VehicleSize = {vehicleCreatedEvent.VehicleSize}");

            var eventData = JsonSerializer.Serialize(vehicleCreatedEvent);

            await _distributedCache.SetStringAsync("VehicleCreatedEventData", eventData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });

            try
            {
                var spot = await _unitOfWork.ParkingRepository.GetAvailableSpotAsync(vehicleCreatedEvent.VehicleSize);

                if (spot == null)
                {
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
                    ZoneName = spot.Zone.Name,
                    PlateNumber = vehicleCreatedEvent.PlateNumber
                };

                var parkingResponseData = JsonSerializer.Serialize(parkingResponse);

                await _distributedCache.SetStringAsync($"ParkingResponse_{vehicleCreatedEvent.PlateNumber}", parkingResponseData, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
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
