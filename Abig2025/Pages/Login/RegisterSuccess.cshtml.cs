using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Abig2025.Services.Interfaces;
using Abig2025.Data;
using Microsoft.EntityFrameworkCore;

namespace Abig2025.Pages.Login
{
    public class RegisterSuccessModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        [BindProperty]
        public string Email { get; set; }

        public RegisterSuccessModel(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        public void OnGet(string email = null)
        {
            Email = email;
        }

        public async Task<IActionResult> OnPostResendVerificationAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["ErrorMessage"] = "Email no proporcionado";
                return Page();
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Email && u.IsActive && !u.IsEmailVerified);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Usuario no encontrado o ya verificado";
                    return Page();
                }

                // Generar nuevo token de verificación
                user.EmailVerificationToken = Guid.NewGuid();
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Enviar email de verificación
                var verificationLink = GenerateVerificationLink(user.EmailVerificationToken.Value);
                var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();
                var emailSent = await emailService.SendVerificationEmailAsync(user.Email, user.FirstName, verificationLink);

                if (emailSent)
                {
                    TempData["SuccessMessage"] = "Se ha reenviado el correo de verificación";
                }
                else
                {
                    TempData["WarningMessage"] = "El usuario fue actualizado pero no se pudo enviar el correo";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al reenviar el correo de verificación";
            }

            return Page();
        }

        private string GenerateVerificationLink(Guid token)
        {
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}/Login/VerifyEmail?token={token}";
        }
    }
}