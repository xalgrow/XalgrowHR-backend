namespace XalgrowHR.Models
{
    public class User
    {
        public int Id { get; set; }

        // Use 'required' if using C# 11+ or initialize with default values otherwise.
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Add the 'Role' property if you need role-based authentication
        public string? Role { get; set; } // Optional (nullable)
    }
}
