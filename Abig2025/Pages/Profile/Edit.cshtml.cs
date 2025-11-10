using Abig2025.Data;
using Abig2025.Helpers;
using Abig2025.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Abig2025.Pages.Profile
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EditProfileInputModel Input { get; set; } = new();

        public string UserFullName { get; set; } = "Usuario";

        public class EditProfileInputModel
        {
            [Required(ErrorMessage = "El nombre es obligatorio")]
            [Display(Name = "Nombre")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "El apellido es obligatorio")]
            [Display(Name = "Apellido")]
            public string LastName { get; set; } = string.Empty;

            // Estos campos NO son requeridos
            public string? Phone { get; set; }
            public string? Dni { get; set; }
            public string? Address { get; set; }
            public string? City { get; set; }
            public string? Province { get; set; }
            public string? Country { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userIdInt);

                if (user == null)
                {
                    return RedirectToPage("/Login");
                }

                UserFullName = $"{user.FirstName} {user.LastName}";

                // Cargar datos existentes
                Input.FirstName = user.FirstName;
                Input.LastName = user.LastName;

                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == userIdInt);

                if (userProfile != null)
                {
                    Input.Phone = userProfile.Phone ?? string.Empty;
                    Input.Dni = userProfile.Dni ?? string.Empty;
                    Input.Address = userProfile.Address ?? string.Empty;
                    Input.City = userProfile.City ?? string.Empty;
                    Input.Province = userProfile.Province ?? string.Empty;
                    Input.Country = userProfile.Country ?? string.Empty;
                }

                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al cargar los datos del perfil.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Obtener userId una sola vez al inicio
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return RedirectToPage("/Login");
            }

            // Validar solo los campos requeridos manualmente
            if (string.IsNullOrWhiteSpace(Input.FirstName))
            {
                ModelState.AddModelError("Input.FirstName", "El nombre es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(Input.LastName))
            {
                ModelState.AddModelError("Input.LastName", "El apellido es obligatorio");
            }

            // Solo verificar si hay errores en los campos requeridos
            if (!ModelState.IsValid)
            {
                // Recargar datos necesarios para la vista
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
                if (user != null)
                {
                    UserFullName = $"{user.FirstName} {user.LastName}";
                }
                return Page();
            }

            try
            {
                // Actualizar usuario
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userIdInt);

                if (user == null)
                {
                    return RedirectToPage("/Login");
                }

                user.FirstName = Input.FirstName.Trim();
                user.LastName = Input.LastName.Trim();
                user.UpdatedAt = HoraArgentina.Now;

                // Actualizar o crear perfil de usuario
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == userIdInt);

                if (userProfile == null)
                {
                    userProfile = new UserProfile
                    {
                        UserId = userIdInt,
                        Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim(),
                        Dni = string.IsNullOrWhiteSpace(Input.Dni) ? null : Input.Dni.Trim(),
                        Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim(),
                        City = string.IsNullOrWhiteSpace(Input.City) ? null : Input.City.Trim(),
                        Province = string.IsNullOrWhiteSpace(Input.Province) ? null : Input.Province.Trim(),
                        Country = string.IsNullOrWhiteSpace(Input.Country) ? null : Input.Country.Trim(),
                    };
                    _context.UserProfiles.Add(userProfile);
                }
                else
                {
                    userProfile.Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim();
                    userProfile.Dni = string.IsNullOrWhiteSpace(Input.Dni) ? null : Input.Dni.Trim();
                    userProfile.Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim();
                    userProfile.City = string.IsNullOrWhiteSpace(Input.City) ? null : Input.City.Trim();
                    userProfile.Province = string.IsNullOrWhiteSpace(Input.Province) ? null : Input.Province.Trim();
                    userProfile.Country = string.IsNullOrWhiteSpace(Input.Country) ? null : Input.Country.Trim();
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
                return RedirectToPage("/Profile/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar el perfil. Por favor, intente nuevamente.");

                // Recargar datos necesarios para la vista
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
                if (user != null)
                {
                    UserFullName = $"{user.FirstName} {user.LastName}";
                }
                return Page();
            }
        }
    }
}