using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using XalgrowHR.Models;
using XalgrowHR.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService; // Handles user-related DB operations
    private readonly IConfiguration _configuration;

    public AuthController(UserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    // DTO for user login
    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // DTO for user registration
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto user)
    {
        if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest(new { message = "Username and Password are required." });
        }

        var authenticatedUser = await _userService.AuthenticateUser(user.Username, user.Password);
        if (authenticatedUser == null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var token = GenerateJwtToken(authenticatedUser);
        return Ok(new { token });
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
        {
            return BadRequest(new { message = "All fields are required." });
        }

        var userRegistered = await _userService.RegisterUser(model.Username, model.Email, model.Password);
        if (userRegistered)
        {
            return Ok(new { message = "User registered successfully." });
        }

        return BadRequest(new { message = "Username or email already exists." });
    }

    // Private method to generate JWT token
    private string GenerateJwtToken(User user)
    {
        if (user == null || string.IsNullOrEmpty(user.Username) || user.Id == 0)
        {
            throw new ArgumentNullException(nameof(user), "User information is missing or invalid.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = _configuration["Jwt:Key"];
        
        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("JWT Key is missing in the configuration.");
        }

        var keyBytes = Encoding.ASCII.GetBytes(key);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role ?? "User") // Default to "User" role if null
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
