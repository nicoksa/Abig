using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Abig2025.Services.Interfaces;
using Abig2025.Models.Properties;

namespace Abig2025.Pages
{
    public class DetailModel : PageModel
    {
        private readonly IPropertyService _propertyService;
        private readonly ILogger<DetailModel> _logger;

        public DetailModel(IPropertyService propertyService, ILogger<DetailModel> logger)
        {
            _propertyService = propertyService;
            _logger = logger;
        }

        public Property? Propiedad { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                Propiedad = await _propertyService.GetPropertyByIdAsync(id);

                if (Propiedad == null)
                {
                    ErrorMessage = "Propiedad no encontrada";
                    _logger.LogWarning("Propiedad con ID {PropertyId} no encontrada", id);
                    return NotFound();
                }

                // Verificar que la propiedad esté activa y publicada
                if (!Propiedad.IsActive || Propiedad.Status?.State != PropertyState.Publicado)
                {
                    ErrorMessage = "Esta propiedad no está disponible";
                    _logger.LogWarning("Intento de acceso a propiedad inactiva o no publicada {PropertyId}", id);
                    return NotFound();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles de propiedad {PropertyId}", id);
                ErrorMessage = "Error al cargar los detalles de la propiedad";
                return Page();
            }
        }
    }
}