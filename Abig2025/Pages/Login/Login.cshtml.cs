// Pages/Login.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Abig2025.Services;
using Abig2025.Models.ViewModels;  
using Abig2025.Models.Users;  
using Abig2025.Services.Interfaces; 

namespace Abig2025.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        //  ViewModel - se usa para el formulario
        [BindProperty]
        public LoginViewModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return Page();  // Si hay errores de validación, muestra la página otra vez
            }

            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();

                // Usamos el ViewModel para obtener los datos
                var (success, user) = await _authService.LoginAsync(
                    Input.Email,
                    Input.Password,
                    ipAddress,
                    userAgent
                );

                if (success)
                {
                    _logger.LogInformation("Usuario {Email} ha iniciado sesión correctamente", Input.Email);

                    // Redirigir al usuario
                    return LocalRedirect(returnUrl ?? "/");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Credenciales inválidas o cuenta no verificada");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el inicio de sesión para {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error durante el inicio de sesión");
                return Page();
            }
        }
    }
}