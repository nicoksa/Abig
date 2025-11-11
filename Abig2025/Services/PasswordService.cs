using Abig2025.Data;
using Abig2025.Helpers;
using Abig2025.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Abig2025.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PasswordService> _logger;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PasswordService(AppDbContext context, ILogger<PasswordService> logger,
                              IEmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool success, string message)> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

                if (user == null)
                {
                    return (true, "Si el email existe, se enviarán instrucciones para resetear la contraseña");
                }

                user.PasswordResetToken = Guid.NewGuid();
                user.PasswordResetTokenExpiry = HoraArgentina.Now.AddHours(2);
                user.UpdatedAt = HoraArgentina.Now;

                await _context.SaveChangesAsync();

                var resetLink = GeneratePasswordResetLink(user.PasswordResetToken.Value, email);

                /*
                _logger.LogInformation("LINK DE RESETEO DE CONTRASEÑA (SIMULADO): {ResetLink}", resetLink);
                */
                // EMAIL REAL DE RECUPERACIÓN
                var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

                if (emailSent)
                {
                    _logger.LogInformation("Email de recuperación enviado exitosamente a {Email}", user.Email);
                    return (true, "Se han enviado las instrucciones para resetear tu contraseña a tu email");
                }

                return (true, "Si el email existe, se enviarán instrucciones para resetear la contraseña");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ForgotPasswordAsync para {Email}", email);
                return (false, "Ha ocurrido un error. Por favor, inténtelo de nuevo.");
            }
        }

        public async Task<(bool success, string message)> ResetPasswordAsync(string token, string email, string newPassword)
        {
            try
            {
                if (!Guid.TryParse(token, out Guid resetToken))
                    return (false, "Token inválido");

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PasswordResetToken == resetToken &&
                                              u.Email == email &&
                                             u.PasswordResetTokenExpiry > HoraArgentina.Now);

                if (user == null)
                    return (false, "Token inválido, expirado o no corresponde al email");

                if (!IsValidPassword(newPassword))
                    return (false, "La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y caracteres especiales");

                CreatePasswordHash(newPassword, out string passwordHash, out string passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.UpdatedAt = HoraArgentina.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña reseteada exitosamente para: {Email}", user.Email);
                return (true, "Contraseña restablecida exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ResetPasswordAsync con token: {Token}", token);
                return (false, "Ha ocurrido un error. Por favor, inténtelo de nuevo.");
            }
        }

        public async Task<(bool success, string message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                    return (false, "Usuario no encontrado");

                if (!VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
                    return (false, "La contraseña actual es incorrecta");

                if (!IsValidPassword(newPassword))
                    return (false, "La nueva contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y caracteres especiales");

                if (VerifyPassword(newPassword, user.PasswordHash, user.PasswordSalt))
                    return (false, "La nueva contraseña no puede ser igual a la anterior");

                CreatePasswordHash(newPassword, out string newPasswordHash, out string newPasswordSalt);

                user.PasswordHash = newPasswordHash;
                user.PasswordSalt = newPasswordSalt;
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.UpdatedAt = HoraArgentina.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña cambiada exitosamente para el usuario: {UserId}", userId);
                return (true, "Contraseña cambiada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ChangePasswordAsync para el usuario {UserId}", userId);
                return (false, "Ha ocurrido un error al cambiar la contraseña");
            }
        }

        public Task<bool> ValidatePasswordAsync(string password)
        {
            return Task.FromResult(IsValidPassword(password));
        }

        private string GeneratePasswordResetLink(Guid token, string email)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}/Login/ResetPassword?token={token}&email={System.Web.HttpUtility.UrlEncode(email)}";
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
    }
}