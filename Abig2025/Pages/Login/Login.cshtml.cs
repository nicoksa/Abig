using Abig2025.Models.Users;
using Abig2025.Models.ViewModels;
using Abig2025.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Abig2025.Pages
{
    [EnableRateLimiting("Login")]
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IExternalAuthService _externalAuthService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthService authService,
                         IExternalAuthService externalAuthService,
                         ILogger<LoginModel> logger)
        {
            _authService = authService;
            _externalAuthService = externalAuthService;
            _logger = logger;
        }

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
                return Page();
            }

            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var (success, user) = await _authService.LoginAsync(
                    Input.Email,
                    Input.Password,
                    ipAddress,
                    userAgent,
                    Input.RememberMe
                );

                if (success)
                {
                    _logger.LogInformation("Usuario {Email} ha iniciado sesión correctamente", Input.Email);
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

        public async Task<IActionResult> OnGetGoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Google");

            if (!result.Succeeded)
            {
                _logger.LogWarning("Falló la autenticación con Google");
                TempData["ErrorMessage"] = "Error al autenticar con Google";
                return RedirectToPage("./Login");
            }

            var (success, message, user) = await _externalAuthService.HandleGoogleLoginAsync(result.Principal);

            if (success && user != null)
            {
                // Crear cookie de autenticación - USAR EL MÉTODO LOCAL
                await CreateAuthenticationCookie(user, false);

                _logger.LogInformation("Login con Google exitoso para: {Email}", user.Email);
                return LocalRedirect(ReturnUrl ?? "/");
            }

            TempData["ErrorMessage"] = message;
            return RedirectToPage("./Login");
        }

        // Acción para iniciar desafío de Google
        public IActionResult OnPostGoogleLogin(string returnUrl = null)
        {
            var redirectUrl = Url.Page("/Login/Login", pageHandler: "GoogleCallback", values: new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        // MÉTODO LOCAL PARA CREAR LA COOKIE
        private async Task CreateAuthenticationCookie(User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("FullName", $"{user.FirstName} {user.LastName}")
            };

            var claimsIdentity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}