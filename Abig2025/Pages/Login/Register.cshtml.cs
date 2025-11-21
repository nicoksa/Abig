// Pages/Login/Register.cshtml.cs
using Abig2025.Models.Users;
using Abig2025.Models.ViewModels;
using Abig2025.Services;
using Abig2025.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        [TempData]
        public string SuccessMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // TEMPORAL: Comentar la validación honeypot para testing
            // if (!string.IsNullOrEmpty(Input.Honeypot))
            // {
            //     _logger.LogWarning("Posible bot detectado en registro");
            //     return RedirectToPage("/Login");
            // }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Verificar si el email ya existe
                if (await _authService.EmailExistsAsync(Input.Email))
                {
                    ModelState.AddModelError("Input.Email", "Este email ya está registrado");
                    return Page();
                }

                // Convertir ViewModel a entidad User
                var user = new User
                {
                    FirstName = Input.FirstName.Trim(),
                    LastName = Input.LastName.Trim(),
                    Email = Input.Email.ToLower().Trim()
                };

                // Crear UserProfile con los datos proporcionados
                var userProfile = new UserProfile
                {
                    Dni = Input.Dni ?? string.Empty,
                    Phone = Input.Phone,
                    Address = Input.Address,
                    City = Input.City,
                    Province = Input.Province,
                    Country = Input.Country,
                    BirthDate = Input.BirthDate
                };

                var (success, message) = await _authService.RegisterAsync(user, Input.Password, userProfile);

                if (success)
                {
                    _logger.LogInformation("Usuario {Email} registrado correctamente", Input.Email);
                    //SuccessMessage = message;
                    return RedirectToPage("/Login/RegisterSuccess", new { email = user.Email });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro para {Email}", Input.Email);
                ModelState.AddModelError(string.Empty, $"Ha ocurrido un error durante el registro: {ex.Message}");
                return Page();
            }
        }


        public IActionResult OnPostGoogleLogin(string returnUrl = null)
        {
            var redirectUrl = Url.Page("/Login/Login", pageHandler: "GoogleCallback", values: new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }
    }
}