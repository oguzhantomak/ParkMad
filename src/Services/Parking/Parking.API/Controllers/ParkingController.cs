namespace Parking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParkingController(IParkingService parkingService) : ControllerBase
{
    [HttpPost("assign")]
    public async Task<IActionResult> AssignParkingSpot([FromBody] ParkingRequestDto request)
    {

        var response = await parkingService.AssignParkingSpotAsync(request);
        return Ok(response);
    }
}