using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Abig2025.Data;
using Microsoft.EntityFrameworkCore;
using Abig2025.Helpers;
using Abig2025.Models.Properties;
using Abig2025.Models.Properties.Enums;

namespace Abig2025.Pages.Profile
{
    [Authorize]
    public class MyPropertiesModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MyPropertiesModel> _logger;

        public MyPropertiesModel(AppDbContext context, ILogger<MyPropertiesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Property> Properties { get; set; } = new();
        public Dictionary<int, string> PropertyStatus { get; set; } = new();
        public int TotalProperties { get; set; }
        public int PublishedCount { get; set; }
        public int DraftCount { get; set; }
        public int PausedCount { get; set; }
        public string UserFullName { get; set; } = string.Empty;

        // Nuevas propiedades para paginación
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TamanoPagina { get; set; } = 6; // 6 propiedades por página
        public bool HayPaginaAnterior => PaginaActual > 1;
        public bool HayPaginaSiguiente => PaginaActual < TotalPaginas;

        public async Task<IActionResult> OnGetAsync(string? filter = null, int pagina = 1)
        {
            try
            {
                // Establecer página actual
                PaginaActual = pagina < 1 ? 1 : pagina;

                // Obtener ID del usuario
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return RedirectToPage("/Login");
                }

                // Obtener nombre del usuario
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user != null)
                {
                    UserFullName = $"{user.FirstName} {user.LastName}";
                }

                // Calcular estadísticas (sin paginación)
                TotalProperties = await _context.Properties
                    .CountAsync(p => p.OwnerId == userId && p.IsActive);

                PublishedCount = await _context.Properties
                    .CountAsync(p => p.OwnerId == userId && p.IsActive &&
                                     p.Status != null && p.Status.State == PropertyState.Publicado);

                DraftCount = await _context.Properties
                    .CountAsync(p => p.OwnerId == userId && p.IsActive &&
                                     (p.Status == null || p.Status.State == PropertyState.Borrador));

                PausedCount = await _context.Properties
                    .CountAsync(p => p.OwnerId == userId && p.IsActive &&
                                     p.Status != null && p.Status.State == PropertyState.Pausado);

                // Consulta base para propiedades (con paginación)
                var query = _context.Properties
                    .Include(p => p.Location)
                    .Include(p => p.Status)
                    .Include(p => p.Images.Where(i => i.IsMain || i.DisplayOrder == 0))
                    .Where(p => p.OwnerId == userId && p.IsActive)
                    .OrderByDescending(p => p.CreatedAt);

                // Calcular total de páginas
                var totalItems = await query.CountAsync();
                TotalPaginas = (int)Math.Ceiling((double)totalItems / TamanoPagina);

                // Ajustar página si es necesario
                if (PaginaActual > TotalPaginas && TotalPaginas > 0)
                    PaginaActual = TotalPaginas;

                // Aplicar paginación
                Properties = await query
                    .Skip((PaginaActual - 1) * TamanoPagina)
                    .Take(TamanoPagina)
                    .ToListAsync();

                // Obtener estados para cada propiedad
                foreach (var property in Properties)
                {
                    var statusText = property.Status?.State switch
                    {
                        PropertyState.Publicado => "Publicado",
                        PropertyState.Pendiente => "Pendiente",
                        PropertyState.Pausado => "Pausado",
                        PropertyState.Rechazado => "Rechazado",
                        PropertyState.Borrador or null => "Borrador",
                        _ => "Desconocido"
                    };

                    PropertyStatus[property.PropertyId] = statusText;
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar propiedades del usuario");
                TempData["ErrorMessage"] = "Error al cargar tus propiedades. Intenta nuevamente.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return RedirectToPage("/Login");
                }

                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == id && p.OwnerId == userId);

                if (property == null)
                {
                    TempData["ErrorMessage"] = "Propiedad no encontrada.";
                    return RedirectToPage();
                }

                // Cambiar a inactivo en lugar de eliminar
                property.IsActive = false;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Propiedad eliminada exitosamente.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar propiedad {PropertyId}", id);
                TempData["ErrorMessage"] = "Error al eliminar la propiedad.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id, string action)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return RedirectToPage("/Login");
                }

                var property = await _context.Properties
                    .Include(p => p.Status)
                    .FirstOrDefaultAsync(p => p.PropertyId == id && p.OwnerId == userId);

                if (property == null)
                {
                    TempData["ErrorMessage"] = "Propiedad no encontrada.";
                    return RedirectToPage();
                }

                var status = property.Status ?? new PropertyStatus { PropertyId = id };

                switch (action.ToLower())
                {
                    case "publish":
                        status.State = PropertyState.Publicado;
                        status.Notes = "Publicado por el usuario";
                        TempData["SuccessMessage"] = "Propiedad publicada exitosamente.";
                        break;

                    case "pause":
                        status.State = PropertyState.Pausado;
                        status.Notes = "Pausado por el usuario";
                        TempData["SuccessMessage"] = "Propiedad pausada.";
                        break;

                    case "unpublish":
                        status.State = PropertyState.Borrador;
                        status.Notes = "Cambiado a borrador";
                        TempData["SuccessMessage"] = "Propiedad movida a borradores.";
                        break;
                }

                status.UpdatedAt = HoraArgentina.Now;

                if (property.Status == null)
                {
                    _context.PropertyStatuses.Add(status);
                }
                else
                {
                    _context.PropertyStatuses.Update(status);
                }

                await _context.SaveChangesAsync();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de propiedad {PropertyId}", id);
                TempData["ErrorMessage"] = "Error al cambiar el estado de la propiedad.";
                return RedirectToPage();
            }
        }
    }
}