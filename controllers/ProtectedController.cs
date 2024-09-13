using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProtectedController : ControllerBase
{
    // Example of a protected endpoint that requires authorization
    [HttpGet("endpoint")]
    public IActionResult GetProtectedData()
    {
        return Ok(new { data = "This is protected data" });
    }
}
