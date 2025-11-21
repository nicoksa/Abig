// ForgotPassword.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Abig2025.Models.ViewModels;
using Abig2025.Services.Interfaces;


namespace Abig2025.Pages.Login
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IPasswordService _passwordService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(IPasswordService passwordService, ILogger<ForgotPasswordModel> logger)
        {
            _passwordService  =passwordService;
            _logger = logger;
        }

        [BindProperty]
        public ForgotPasswordViewModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var (success, message, email) = await _passwordService.ForgotPasswordAsync(Input.Email);

                if (success)
                {
                    StatusMessage = message;
                    return RedirectToPage("./ForgotPasswordConfirmation", new { email = email });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la solicitud de recuperación para {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error. Por favor, inténtelo de nuevo.");
                return Page();
            }
        }
    }
}
