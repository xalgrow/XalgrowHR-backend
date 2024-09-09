using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLoginDto user)
    {
        if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("Username or Password is missing.");
        }

        if (IsValidUser(user))
        {
            var token = GenerateJWT(user.Username);
            return Ok(new { token });
        }

        return Unauthorized();
    }

    private bool IsValidUser(UserLoginDto user)
    {
        // Replace this with real user authentication logic (e.g., check a database)
        return user.Username == "poweruser" && user.Password == "password";
    }

    private string GenerateJWT(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "PowerUser") // Replace with actual role fetching logic if needed
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(120),  // Adjust expiration as needed
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
