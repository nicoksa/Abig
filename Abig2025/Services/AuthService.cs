using Abig2025.Data;
using Abig2025.Models.Users;

using Abig2025.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Abig2025.Models.ViewModels;

namespace Abig2025.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(AppDbContext context, ILogger<AuthService> logger,
                      IEmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool success, string message)> RegisterAsync(User user, string password, UserProfile? userProfile = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validaciones adicionales
                if (await EmailExistsAsync(user.Email))
                    return (false, "Este email ya está registrado");

                if (!IsValidPassword(password))
                    return (false, "La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y caracteres especiales");

                // Crear hash de contraseña
                CreatePasswordHash(password, out string passwordHash, out string passwordSalt);

                // Configurar usuario
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.EmailVerificationToken = Guid.NewGuid();
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Primer SaveChanges para obtener el UserId

                // CREAR PERFIL DE USUARIO (fijo, no condicional)
                var profileToCreate = userProfile ?? new UserProfile();
                profileToCreate.UserId = user.UserId;

                // Asegurar que Dni no sea null
                if (string.IsNullOrEmpty(profileToCreate.Dni))
                    profileToCreate.Dni = string.Empty;

                _context.UserProfiles.Add(profileToCreate);

                // Asignar rol por defecto
                await AssignDefaultRoleAsync(user.UserId);

                // Guardar TODO en una sola transacción
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Enviar email de verificación (simulado)
                var verificationLink = GenerateVerificationLink(user.EmailVerificationToken.Value);
                _logger.LogInformation("EMAIL DE VERIFICACIÓN (SIMULADO): {VerificationLink}", verificationLink);
                var emailSent = true; // Simular que se envió

                _logger.LogInformation("Usuario registrado exitosamente: {Email}", user.Email);
                return (true, "Registro exitoso. Por favor verifica tu email para activar tu cuenta.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error durante el registro para {Email}", user.Email);
                return (false, "Ha ocurrido un error durante el registro. Por favor, inténtelo de nuevo.");
            }
        }

        private string GenerateVerificationLink(Guid token)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}/Login/VerifyEmail?token={token}";
        }

        public async Task<bool> SendVerificationEmailAsync(User user)
        {
            // TEMPORAL: Implementación simulada
            _logger.LogInformation("SIMULACIÓN: Email de verificación enviado a {Email}", user.Email);
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            try
            {
                if (!Guid.TryParse(token, out Guid verificationToken))
                    return false;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailVerificationToken == verificationToken &&
                                             u.EmailVerificationTokenExpiry > DateTime.UtcNow);

                if (user == null)
                    return false;

                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Email verificado exitosamente para: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando email con token: {Token}", token);
                return false;
            }
        }

        private async Task AssignDefaultRoleAsync(int userId)
        {
            try
            {
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                if (defaultRole == null)
                {
                    // Crear rol por defecto si no existe
                    defaultRole = new Role
                    {
                        RoleName = "User",
                        Description = "Usuario estándar"
                    };
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asignando rol por defecto al usuario {UserId}", userId);
                throw;
            }
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            var hasUpperCase = password.Any(char.IsUpper);
            var hasLowerCase = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPassword(string password, string storedHash, string salt)
        {
            try
            {
                var saltBytes = Convert.FromBase64String(salt);
                using var hmac = new HMACSHA512(saltBytes);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(Convert.FromBase64String(storedHash));
            }
            catch
            {
                return false;
            }
        }

        // Mantener los métodos existentes...
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

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await LogLoginAttemptAsync(email, true, ipAddress, userAgent, user.UserId);
            return (true, user);
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
    }
}