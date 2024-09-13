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
using System.Security.Cryptography;

namespace XalgrowHR.Controllers
{
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
            public string? Username { get; set; } // Made nullable
            public string? Password { get; set; } // Made nullable
        }

        // DTO for user registration
        public class RegisterModel
        {
            public string? Username { get; set; } // Made nullable
            public string? Email { get; set; } // Made nullable
            public string? Password { get; set; } // Made nullable
        }

        // DTO for refresh token request
        public class RefreshTokenRequest
        {
            public string? Token { get; set; } // Made nullable
            public string? RefreshToken { get; set; } // Made nullable
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto? user)
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

            var accessToken = GenerateJwtToken(authenticatedUser);
            var refreshToken = GenerateRefreshToken();
            authenticatedUser.RefreshToken = refreshToken;
            authenticatedUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days expiry

            await _userService.UpdateUserAsync(authenticatedUser);

            return Ok(new
            {
                Token = accessToken,
                RefreshToken = refreshToken
            });
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel? model)
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

        // POST: api/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request)
        {
            if (request == null || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { message = "Invalid request." });
            }

            var principal = GetPrincipalFromExpiredToken(request.Token);
            var username = principal?.Identity?.Name;
            var user = await _userService.GetUserByUsernameAsync(username ?? string.Empty);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Invalid refresh token or token expired." });
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userService.UpdateUserAsync(user);

            return Ok(new
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        // Private method to generate JWT token
        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "default-key");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Audience = _configuration["Jwt:Audience"],  // Set the audience here
                Issuer = _configuration["Jwt:Issuer"],      // Set the issuer here
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Private method to generate a secure refresh token
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        // Private method to get claims from an expired JWT token
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing in the configuration.");

            var keyBytes = Encoding.ASCII.GetBytes(key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Don't validate expiration for refresh tokens
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"]
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);
            var jwtToken = securityToken as JwtSecurityToken;

            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
