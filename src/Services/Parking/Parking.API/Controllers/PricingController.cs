using Microsoft.AspNetCore.Mvc;

namespace Parking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PricingController : ControllerBase
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}