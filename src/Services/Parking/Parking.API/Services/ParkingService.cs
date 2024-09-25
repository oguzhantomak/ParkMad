namespace Parking.API.Services
{
    public class ParkingService : IParkingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ParkingService> _logger;

        public ParkingService(
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILogger<ParkingService> logger,
            IEventPublisher eventPublisher)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;

            eventPublisher.Subscribe<VehicleCreatedEvent>(HandleVehicleCreatedEvent);
        }

        private Task HandleVehicleCreatedEvent(VehicleCreatedEvent vehicleCreatedEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Received VehicleCreatedEvent: PlateNumber = {vehicleCreatedEvent.PlateNumber}, VehicleSize = {vehicleCreatedEvent.VehicleSize}");

            _cache.Set("VehicleCreatedEventData", vehicleCreatedEvent, TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }


        public async Task<ParkingResponseDto> AssignParkingSpotAsync(ParkingRequestDto request)
        {
            if (_cache.TryGetValue("VehicleCreatedEventData", out VehicleCreatedEvent vehicleCreatedEvent))
            {

                _logger.LogInformation($"Processing parking spot assignment for: PlateNumber = {vehicleCreatedEvent.PlateNumber}, VehicleSize = {vehicleCreatedEvent.VehicleSize}");

                var cacheKey = $"AvailableSpot_{vehicleCreatedEvent.VehicleSize}";

                if (!_cache.TryGetValue(cacheKey, out ParkingSpot spot))
                {
                    spot = await _unitOfWork.ParkingRepository.GetAvailableSpotAsync(vehicleCreatedEvent.VehicleSize);

                    if (spot == null)
                    {
                        _logger.LogWarning("Uygun park yeri bulunamadı.");
                        throw new Exception("Uygun park yeri bulunamadı.");
                    }

                    _cache.Set(cacheKey, spot, TimeSpan.FromMinutes(5));
                }

                spot.IsOccupied = true;
                spot.OccupiedAt = DateTime.UtcNow;
                _unitOfWork.ParkingRepository.Update(spot);

                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Atanan park yeri ID: {SpotId}", spot.Id);

                //return _mapper.Map<ParkingResponseDto>(spot);
                return new ParkingResponseDto
                {
                    SpotId = spot.Id,
                    AssignedAt = (DateTime)spot.OccupiedAt,
                    ZoneName = spot.Zone.Name
                };
            }
            else
            {
                _logger.LogWarning("No VehicleCreatedEventData found in cache.");
            }

            throw new Exception("Uygun park yeri bulunamadı.");
        }
    }
}