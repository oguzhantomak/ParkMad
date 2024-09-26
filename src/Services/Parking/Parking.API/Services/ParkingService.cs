using Parking.API.Events;
using Polly;

namespace Parking.API.Services
{
    public class ParkingService : IParkingService, IConsumer<VehicleCreatedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<ParkingService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public ParkingService(
            IUnitOfWork unitOfWork,
            IDistributedCache distributedCache,
            ILogger<ParkingService> logger, IPublishEndpoint publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _distributedCache = distributedCache;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
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
                spot.OccupiedBy = vehicleCreatedEvent.PlateNumber;
                _unitOfWork.ParkingRepository.Update(spot);

                await _unitOfWork.CompleteAsync();

                var parkingResponse = new ParkingResponseDto
                {
                    SpotId = spot.Id,
                    AssignedAt = DateTime.UtcNow,
                    ZoneName = spot.Zone.Name,
                    PlateNumber = vehicleCreatedEvent.PlateNumber,
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
                var spot = await _unitOfWork.ParkingRepository.GetSpotByPlateNumber(request.PlateNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            throw new NotImplementedException();
        }

        public async Task<UnparkResponseDto> ReleaseParkingSpotAsync(UnparkRequestDto request)
        {
            var spot = await _unitOfWork.ParkingRepository.GetSpotByPlateNumber(request.PlateNumber);

            if (spot is not null)
            {
                var pricingRequest = new PricingRequestEvent
                {
                    PlateNumber = request.PlateNumber,
                    Duration = (DateTime.UtcNow - spot.OccupiedAt.Value).TotalHours,
                    ZoneName = spot.Zone.Name,
                };

                await _publishEndpoint.Publish(pricingRequest);

                var policy = Policy.HandleResult<string>(result => result == null)
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(2), (result, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retrying to get Pricing data from cache... Attempt: {retryCount}");
                    });

                var cachedPrice = await policy.ExecuteAsync(async () =>
                {
                    return await _distributedCache.GetStringAsync($"PricingResponse_{request.PlateNumber}");
                });

                if (!string.IsNullOrEmpty(cachedPrice))
                {
                    var priceResponse = JsonSerializer.Deserialize<PriceResponseDto>(cachedPrice);
                    return new UnparkResponseDto
                    {
                        Price = priceResponse.Price,
                        TotalHours = pricingRequest.Duration
                    };
                }

                _logger.LogError("Failed to retrieve Pricing data from cache.");
                throw new Exception("Pricing information could not be retrieved.");
            }

            throw new Exception("Parking spot not found.");
        }

    }
}
