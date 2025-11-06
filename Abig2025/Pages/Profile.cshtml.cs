using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Abig2025.Data;
using Microsoft.EntityFrameworkCore;

namespace Abig2025.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly AppDbContext _context;

        public ProfileModel(AppDbContext context)
        {
            _context = context;
        }

        public string UserFullName { get; set; } = "Usuario";
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string MemberSince { get; set; } = string.Empty;
        public int PropertiesCount { get; set; }
        public int FavoritesCount { get; set; }
        public int MemberFor { get; set; }
        public int LastLoginDays { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Cargar información del usuario
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userIdInt);

                if (user == null)
                {
                    return RedirectToPage("/Login");
                }

                // Información básica del usuario
                UserFullName = $"{user.FirstName} {user.LastName}";
                Email = user.Email;
                MemberSince = user.CreatedAt.ToString("dd/MM/yyyy");
                MemberFor = (int)(DateTime.UtcNow - user.CreatedAt).TotalDays;
                LastLoginDays = user.LastLogin.HasValue ?
                    (int)(DateTime.UtcNow - user.LastLogin.Value).TotalDays : 0;

                // Cargar información del perfil si existe
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == userIdInt);

                if (userProfile != null)
                {
                    Phone = userProfile.Phone ?? string.Empty;
                    Dni = userProfile.Dni ?? string.Empty;
                    Address = userProfile.Address ?? string.Empty;
                    City = userProfile.City ?? string.Empty;
                    Province = userProfile.Province ?? string.Empty;
                    Country = userProfile.Country ?? string.Empty;
                }

                // Contar propiedades - si tienes el modelo Property
                try
                {
                    PropertiesCount = await _context.Properties
                        .CountAsync(p => p.OwnerId == userIdInt);
                }
                catch
                {
                    PropertiesCount = 0; // Si no existe el modelo Property
                }

                // Favoritos - puedes implementarlo después
                FavoritesCount = 0;

                return Page();
            }
            catch (Exception ex)
            {
                // Si hay error, mostrar página con datos básicos
                UserFullName = User.FindFirstValue(ClaimTypes.Name) ?? "Usuario";
                Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                MemberSince = DateTime.UtcNow.ToString("dd/MM/yyyy");
                return Page();
            }
        }
    }
}