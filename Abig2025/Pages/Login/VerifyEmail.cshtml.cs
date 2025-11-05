using Abig2025.Services;
using Abig2025.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Abig2025.Pages.Login
{
    public class VerifyEmailModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<VerifyEmailModel> _logger;

        public VerifyEmailModel(IAuthService authService, ILogger<VerifyEmailModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Token))
            {
                Message = "Token de verificación no proporcionado";
                return Page();
            }

            try
            {
                IsSuccess = await _authService.VerifyEmailAsync(Token);

                if (IsSuccess)
                {
                    Message = "¡Email verificado exitosamente! Ya puedes iniciar sesión.";
                    _logger.LogInformation("Email verificado con token: {Token}", Token);
                }
                else
                {
                    Message = "Token inválido o expirado. Por favor, solicita un nuevo enlace de verificación.";
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando email con token: {Token}", Token);
                Message = "Ha ocurrido un error durante la verificación. Por favor, inténtelo de nuevo.";
                return Page();
            }
        }
    }
}
