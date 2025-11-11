using Abig2025.Data;
using Abig2025.Helpers;
using Abig2025.Models.Users;
using Abig2025.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Abig2025.Services
{
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExternalAuthService> _logger;

        public ExternalAuthService(AppDbContext context, ILogger<ExternalAuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool success, string message, User user)> HandleGoogleLoginAsync(ClaimsPrincipal principal)
        {
            try
            {
                var googleId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var firstName = principal.FindFirst(ClaimTypes.GivenName)?.Value;
                var lastName = principal.FindFirst(ClaimTypes.Surname)?.Value;
                var name = principal.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
                {
                    return (false, "Información de Google incompleta", null);
                }

                // Buscar usuario existente
                var existingUser = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.GoogleId == googleId || u.Email == email);

                if (existingUser != null)
                {
                    // Si el usuario existe pero no tiene GoogleId, actualizarlo
                    if (string.IsNullOrEmpty(existingUser.GoogleId))
                    {
                        existingUser.GoogleId = googleId;
                    }

                    // Actualizar último login
                    existingUser.LastLogin = HoraArgentina.Now;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Usuario de Google logueado: {Email}", email);
                    return (true, "Login exitoso", existingUser);
                }

                // Crear nuevo usuario
                var newUser = await CreateUserFromGoogleAsync(googleId, email, firstName, lastName, name);
                return (true, "Usuario registrado y logueado exitosamente", newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login con Google");
                return (false, "Error durante el login con Google", null);
            }
        }

        public async Task<bool> IsGoogleUserRegisteredAsync(string googleId)
        {
            return await _context.Users.AnyAsync(u => u.GoogleId == googleId);
        }

        public async Task<User> CreateUserFromGoogleAsync(string googleId, string email, string firstName, string lastName, string fullName = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Si no tenemos nombre y apellido separados, intentar dividir el nombre completo
                if (string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(fullName))
                {
                    var nameParts = fullName.Split(' ');
                    firstName = nameParts[0];
                    lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";
                }


                // Generar hash y salt dummy para usuarios de Google
                // Uso valores que indiquen que es una cuenta de Google
                string dummyPassword = Guid.NewGuid().ToString() + "GoogleAuth123!";
                CreatePasswordHash(dummyPassword, out string passwordHash, out string passwordSalt);


                var user = new User
                {
                    GoogleId = googleId,
                    Email = email,
                    FirstName = firstName ?? "Usuario",
                    LastName = lastName ?? "Google",
                    PasswordHash = passwordHash, 
                    PasswordSalt = passwordSalt,
                    IsEmailVerified = true, // Google ya verificó el email
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastLogin = HoraArgentina.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Crear perfil de usuario
                var userProfile = new UserProfile
                {
                    UserId = user.UserId,
                    Dni = string.Empty
                };
                _context.UserProfiles.Add(userProfile);

                // Asignar rol por defecto
                await AssignDefaultRoleAsync(user.UserId);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Nuevo usuario creado desde Google: {Email}", email);
                return user;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creando usuario desde Google: {Email}", email);
                throw;
            }
        }

        private async Task AssignDefaultRoleAsync(int userId)
        {
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (defaultRole == null)
            {
                defaultRole = new Role { RoleName = "User", Description = "Usuario estándar" };
                _context.Roles.Add(defaultRole);
                await _context.SaveChangesAsync();
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = defaultRole.RoleId,
                AssignedAt = DateTime.UtcNow
            };
            _context.UserRoles.Add(userRole);
        }



        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}
