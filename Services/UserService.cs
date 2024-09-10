using Microsoft.EntityFrameworkCore;
using BCrypt.Net; // To handle password hashing
using System.Threading.Tasks;
using XalgrowHR.Models;
using XalgrowHR.Data;

namespace XalgrowHR.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // Method to authenticate a user with their username and password
        public async Task<User?> AuthenticateUser(string username, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);

            // Check if user exists and password matches
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Invalid username or password
                return null;
            }

            // Return the authenticated user
            return user;
        }

        // Method to register a new user
        public async Task<bool> RegisterUser(string username, string email, string password)
        {
            // Check if the username or email already exists in the database
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
            {
                // Username or email already exists
                return false;
            }

            // Hash the password before storing it in the database
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Create a new user object with the provided details
            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
            };

            // Add the new user to the database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Registration successful
            return true;
        }
    }
}
