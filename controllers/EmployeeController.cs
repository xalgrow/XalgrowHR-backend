using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    // GET: api/employee
    [HttpGet]
    public IActionResult GetAllEmployees()
    {
        // Here you can return some sample employee data for now
        var employees = new List<object>
        {
            new { Id = 1, Name = "John Doe", Position = "Developer" },
            new { Id = 2, Name = "Jane Smith", Position = "Manager" }
        };
        
        return Ok(employees);
    }
}
