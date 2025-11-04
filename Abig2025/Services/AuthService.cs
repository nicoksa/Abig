// Services/AuthService.cs
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Abig2025.Data;
using Abig2025.Models.Users;

namespace Abig2025.Services
{
    public interface IAuthService
    {
        Task<(bool success, User user)> LoginAsync(string email, string password, string ipAddress, string userAgent);
        Task<bool> RegisterAsync(User user, string password);
        Task<bool> EmailExistsAsync(string email);
        Task LogLoginAttemptAsync(string email, bool isSuccessful, string ipAddress, string userAgent, int? userId = null);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, User user)> LoginAsync(string email, string password, string ipAddress, string userAgent)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null)
            {
                await LogLoginAttemptAsync(email, false, ipAddress, userAgent);
                return (false, null);
            }

            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                await LogLoginAttemptAsync(email, false, ipAddress, userAgent, user.UserId);
                return (false, null);
            }

            if (!user.IsEmailVerified)
            {
                await LogLoginAttemptAsync(email, false, ipAddress, userAgent, user.UserId);
                return (false, null);
            }

            // Actualizar último login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await LogLoginAttemptAsync(email, true, ipAddress, userAgent, user.UserId);
            return (true, user);
        }

        public async Task<bool> RegisterAsync(User user, string password)
        {
            if (await EmailExistsAsync(user.Email))
                return false;

            CreatePasswordHash(password, out string passwordHash, out string passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.EmailVerificationToken = Guid.NewGuid();
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Asignar rol por defecto (Usuario)
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (defaultRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.UserId,
                    RoleId = defaultRole.RoleId,
                    AssignedAt = DateTime.UtcNow
                };
                _context.UserRoles.Add(userRole);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task LogLoginAttemptAsync(string email, bool isSuccessful, string ipAddress, string userAgent, int? userId = null)
        {
            var attempt = new LoginAttempt
            {
                UserId = userId,
                Email = email,
                AttemptDate = DateTime.UtcNow,
                IsSuccessful = isSuccessful,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.LoginAttempts.Add(attempt);
            await _context.SaveChangesAsync();
        }

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            using var hmac = new HMACSHA512(saltBytes);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(Convert.FromBase64String(storedHash));
        }
    }
}
