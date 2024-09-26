namespace Parking.API.Services;

public interface IParkingService
{
    Task<ParkingResponseDto> AssignParkingSpotAsync(ParkingRequestDto request);
    Task<UnparkResponseDto> ReleaseParkingSpotAsync(UnparkRequestDto request);
}