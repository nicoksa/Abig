using Abig2025.Data;
using Abig2025.Helpers;
using Abig2025.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Dni { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Province { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return RedirectToPage("/Login");
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

                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.UpdatedAt = HoraArgentina.Now;

                // Actualizar o crear perfil de usuario
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == userIdInt);

                if (userProfile == null)
                {
                    userProfile = new UserProfile
                    {
                        UserId = userIdInt,
                        Phone = Input.Phone,
                        Dni = Input.Dni,
                        Address = Input.Address,
                        City = Input.City,
                        Province = Input.Province,
                        Country = Input.Country,
                       
                    };
                    _context.UserProfiles.Add(userProfile);
                }
                else
                {
                    userProfile.Phone = Input.Phone;
                    userProfile.Dni = Input.Dni;
                    userProfile.Address = Input.Address;
                    userProfile.City = Input.City;
                    userProfile.Province = Input.Province;
                    userProfile.Country = Input.Country;
                   
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
                return RedirectToPage("/Profile/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar el perfil. Por favor, intente nuevamente.");
                return Page();
            }
        }
    }
}
