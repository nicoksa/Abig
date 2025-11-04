// Pages/Login/Register.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Abig2025.Services;
using Abig2025.Models.ViewModels;
using Abig2025.Models.Users;

namespace Abig2025.Pages.Login
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Verificar si el email ya existe
            if (await _authService.EmailExistsAsync(Input.Email))
            {
                ModelState.AddModelError("Input.Email", "Este email ya está registrado");
                return Page();
            }

            try
            {
                // Convertir el ViewModel a la entidad User
                var user = new User
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email
                    // PasswordHash y Salt los genera el AuthService
                };

                var result = await _authService.RegisterAsync(user, Input.Password);

                if (result)
                {
                    _logger.LogInformation("Usuario {Email} registrado correctamente", Input.Email);

                    // Redirigir a página de éxito o login
                    TempData["SuccessMessage"] = "Registro exitoso. Por favor verifica tu email.";
                    return RedirectToPage("/Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al crear la cuenta");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro para {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, "Ha ocurrido un error durante el registro");
                return Page();
            }
        }
    }
}