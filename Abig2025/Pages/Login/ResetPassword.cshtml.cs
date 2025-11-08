// ResetPassword.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Abig2025.Models.ViewModels;
using Abig2025.Services.Interfaces;

namespace Abig2025.Pages.Login
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IPasswordService _passwordService;
        private readonly ILogger<ResetPasswordModel> _logger;

        public ResetPasswordModel(IPasswordService passwordService, ILogger<ResetPasswordModel> logger)
        {
            _passwordService = passwordService;
            _logger = logger;
        }

        [BindProperty]
        public ResetPasswordViewModel Input { get; set; }

        public IActionResult OnGet(string token, string email)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("./Login");
            }

            Input = new ResetPasswordViewModel { Token = token, Email = email };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var (success, message) = await _passwordService.ResetPasswordAsync(Input.Token, Input.Email, Input.NewPassword);

                if (success)
                {
                    return RedirectToPage("./ResetPasswordConfirmation");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el reseteo de contraseña para {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error. Por favor, inténtelo de nuevo.");
                return Page();
            }
        }
    }
}
