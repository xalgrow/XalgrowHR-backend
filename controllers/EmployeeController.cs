using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using XalgrowHR.Models; // Import the Employee model from this namespace
using XalgrowHR.Data;
using BCrypt.Net; // Add this to use BCrypt for password hashing
using Microsoft.AspNetCore.Authorization; // Add this to use [Authorize] attribute

[Authorize] // Secures the entire controller
[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;
    private readonly FileStorageService _fileStorageService;

    public EmployeeController(AppDbContext context, EmailService emailService, FileStorageService fileStorageService)
    {
        _context = context;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
    }

    // GET: api/employee
    [HttpGet]
    public async Task<IActionResult> GetEmployees([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var query = _context.Employees.Include(e => e.Department).AsQueryable();

        // Filter by search term if provided
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e => e.Name.Contains(search) || e.Position.Contains(search));
        }

        // Get the total count of employees
        var totalEmployees = await query.CountAsync();

        // Apply pagination
        var employees = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Return the paginated result
        return Ok(new
        {
            TotalCount = totalEmployees,
            PageSize = pageSize,
            CurrentPage = pageNumber,
            Employees = employees
        });
    }

    // POST: api/employee/create
    [Authorize(Roles = "PowerUser")]  // Ensures only PowerUser can create employees
    [HttpPost("create")]
    public IActionResult CreateEmployee(Employee employee)
    {
        if (employee == null)
        {
            return BadRequest(new { message = "Invalid employee data." });
        }

        // Validate Department presence
        if (employee.DepartmentId == null)
        {
            return BadRequest(new { message = "Department is required." });
        }

        // Convert date fields to UTC before saving to the database
        employee.DateOfBirth = employee.DateOfBirth?.ToUniversalTime();
        employee.HireDate = employee.HireDate?.ToUniversalTime();

        // Save employee to the database
        _context.Employees.Add(employee);
        _context.SaveChanges();

        // Return the newly created employee
        return Ok(employee);
    }

    // POST: api/employee/upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadEmployeeFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or not provided.");
        }

        // Check file size against maximum allowed size
        if (file.Length > _fileStorageService.MaxFileSizeMB * 1024 * 1024)
        {
            return BadRequest("File is too large.");
        }

        var filePath = await _fileStorageService.SaveFileAsync(file);

        return Ok(new { filePath });
    }

    // POST: api/employee/login
    [AllowAnonymous] // Allow login without being authenticated
    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string plainTextPassword)
    {
        var employee = await _context.Employees.SingleOrDefaultAsync(e => e.Username == username);

        if (employee == null || string.IsNullOrEmpty(employee.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        // Verify the hashed password with the plain text password provided
        if (!BCrypt.Net.BCrypt.Verify(plainTextPassword, employee.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        // Proceed with successful login (e.g., generate JWT token, etc.)
        return Ok(new { message = "Login successful" });
    }

    // GET: api/employee/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var employee = await _context.Employees.Include(e => e.Department).FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    // PUT: api/employee/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest();
        }

        _context.Entry(employee).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/employee/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
}
