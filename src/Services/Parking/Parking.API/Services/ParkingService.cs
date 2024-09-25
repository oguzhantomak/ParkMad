namespace Parking.API.Services
{
    public class ParkingService(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<ParkingService> logger,
        IPublishEndpoint publishEndpoint)
        : IParkingService, IConsumer<VehicleCreatedEvent>
    {
        public async Task Consume(ConsumeContext<VehicleCreatedEvent> context)
        {
            var vehicleCreatedEvent = context.Message;
            logger.LogInformation($"Received VehicleCreatedEvent: PlateNumber = {vehicleCreatedEvent.PlateNumber}, VehicleSize = {vehicleCreatedEvent.VehicleSize}");

            cache.Set("VehicleCreatedEventData", vehicleCreatedEvent, TimeSpan.FromMinutes(2));

            try
            {
                var spot = await unitOfWork.ParkingRepository.GetAvailableSpotAsync(vehicleCreatedEvent.VehicleSize);

                if (spot == null)
                {
                    //TODO: Rollback eventı fırlatılacak!

                    logger.LogWarning("Uygun park yeri bulunamadı.");
                    throw new Exception("Uygun park yeri bulunamadı.");
                }

                spot.IsOccupied = true;
                spot.OccupiedAt = DateTime.UtcNow;
                unitOfWork.ParkingRepository.Update(spot);

                await unitOfWork.CompleteAsync();

                var parkingResponse = new ParkingResponseDto
                {
                    SpotId = spot.Id,
                    AssignedAt = DateTime.UtcNow,
                    ZoneName = spot.Zone.Name
                };

                await publishEndpoint.Publish(parkingResponse);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                throw;
            }
            await Task.CompletedTask;
        }

        public async Task<ParkingResponseDto> AssignParkingSpotAsync(ParkingRequestDto request)
        {
            throw new NotImplementedException();
        }
    }
}
