using Abig2025.Data;
using Abig2025.Helpers;
using Abig2025.Models.Users;
using Abig2025.Models.ViewModels;
using Abig2025.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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

                /*
                // Enviar email de verificación (simulado)
                var verificationLink = GenerateVerificationLink(user.EmailVerificationToken.Value);
                _logger.LogInformation("EMAIL DE VERIFICACIÓN (SIMULADO): {VerificationLink}", verificationLink);
                var emailSent = true; // Simular que se envió
                */

                // EMAIL DE VERIFICACIÓN REAL
                var verificationLink = GenerateVerificationLink(user.EmailVerificationToken.Value);
                var emailSent = await _emailService.SendVerificationEmailAsync(user.Email, user.FirstName, verificationLink);


                if (!emailSent)
                {
                    _logger.LogWarning("No se pudo enviar el email de verificación a {Email}, pero el usuario fue registrado", user.Email);
                }


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
                                             u.EmailVerificationTokenExpiry > HoraArgentina.Now);

                if (user == null)
                    return false;

                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;
                user.UpdatedAt = HoraArgentina.Now;

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


        public async Task<(bool success, User user)> LoginAsync(string email, string password, string ipAddress, string userAgent, bool rememberMe = false)
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

            if (!string.IsNullOrEmpty(user.GoogleId))
            {
                // Usuario registrado con Google, no puede hacer login con contraseña
                await LogLoginAttemptAsync(email, false, ipAddress, userAgent, user.UserId);
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

            // ACTUALIZAR ÚLTIMO LOGIN
            user.LastLogin = HoraArgentina.Now;
            await _context.SaveChangesAsync();

            // CREAR COOKIE DE AUTENTICACIÓN
            await CreateAuthenticationCookie(user, rememberMe);

            await LogLoginAttemptAsync(email, true, ipAddress, userAgent, user.UserId);
            return (true, user);
        }


        private async Task CreateAuthenticationCookie(User user, bool rememberMe)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                    new Claim("FullName", $"{user.FirstName} {user.LastName}")
                };

                // Agregar roles a los claims
                foreach (var userRole in user.UserRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe, // Esto hará que la cookie persista entre sesiones del navegador
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                    AllowRefresh = true
                };

                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("Cookie de autenticación creada para el usuario: {Email} - RememberMe: {RememberMe}", user.Email, rememberMe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando cookie de autenticación para: {Email}", user.Email);
                throw;
            }
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

        public async Task LogoutAsync()
        {
            try
            {
                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("Usuario cerró sesión exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el logout");
                throw;
            }
        }


        public async Task<(bool success, string message)> ResendVerificationEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive && !u.IsEmailVerified);

                if (user == null)
                    return (false, "Usuario no encontrado o ya verificado");

                // Generar nuevo token
                user.EmailVerificationToken = Guid.NewGuid();
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Enviar email
                var verificationLink = GenerateVerificationLink(user.EmailVerificationToken.Value);
                var emailSent = await _emailService.SendVerificationEmailAsync(user.Email, user.FirstName, verificationLink);

                if (!emailSent)
                {
                    _logger.LogWarning("No se pudo enviar el email de verificación a {Email}", user.Email);
                    return (false, "No se pudo enviar el correo de verificación");
                }

                _logger.LogInformation("Email de verificación reenviado a: {Email}", user.Email);
                return (true, "Correo de verificación reenviado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reenviando email de verificación a {Email}", email);
                return (false, "Error al reenviar el correo de verificación");
            }
        }
    }
}