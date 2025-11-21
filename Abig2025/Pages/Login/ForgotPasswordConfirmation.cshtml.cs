using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Abig2025.Services.Interfaces;

namespace Abig2025.Pages.Login
{
    public class ForgotPasswordConfirmationModel : PageModel
    {
        private readonly IPasswordService _passwordService;

        [BindProperty]
        public string Email { get; set; }

        public ForgotPasswordConfirmationModel(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        public void OnGet(string email = null)
        {
            Email = email;
            ViewData["Email"] = email;
        }

        public async Task<IActionResult> OnPostResendAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["ErrorMessage"] = "Email no proporcionado";
                return Page();
            }

            var (success, message, _) = await _passwordService.ForgotPasswordAsync(Email);

            if (success)
            {
                TempData["SuccessMessage"] = "Se ha reenviado el correo de recuperación";
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return Page();
        }
    }
}
