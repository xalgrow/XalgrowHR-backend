using Microsoft.EntityFrameworkCore;
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

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null; // Invalid username or password
            }

            return user;
        }

        // Method to get a user by username
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
        }

        // Method to register a new user
        public async Task<bool> RegisterUser(string username, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
            {
                return false; // Username or email already exists
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return true; // Registration successful
        }

        // Method to update the user (e.g., to save the refresh token)
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
