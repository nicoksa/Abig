using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Abig2025.Services.Interfaces;

namespace Abig2025.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(IAuthService authService, ILogger<LogoutModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _authService.LogoutAsync();
            _logger.LogInformation("Usuario cerró sesión exitosamente");
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnGet()
        {
            await _authService.LogoutAsync();
            _logger.LogInformation("Usuario cerró sesión exitosamente");
            return RedirectToPage("/Index");
        }
    }
}