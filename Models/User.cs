using System;

namespace XalgrowHR.Models
{
    public class User
    {
        public int Id { get; set; } // Primary Key
        public string Username { get; set; } = string.Empty; // Username
        public string Email { get; set; } = string.Empty; // Email
        public string PasswordHash { get; set; } = string.Empty; // Password hash
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Created timestamp
        public string? Role { get; set; } // Optional Role (nullable)
        public string? RefreshToken { get; set; } // Optional Refresh Token (nullable)
        public DateTime? RefreshTokenExpiryTime { get; set; } // Refresh token expiry time (nullable)
    }
}
