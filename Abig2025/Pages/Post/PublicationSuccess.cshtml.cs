using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Abig2025.Data;
using Microsoft.EntityFrameworkCore;
using Abig2025.Helpers;
using Abig2025.Models.Properties;

namespace Abig2025.Pages.Post
{
    public class PublicationSuccessModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PublicationSuccessModel> _logger;

        public Property? Property { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string PublicationCode { get; set; } = string.Empty;
        public string NextStepsGuide { get; set; } = string.Empty;

        public PublicationSuccessModel(AppDbContext context, ILogger<PublicationSuccessModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int propertyId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                {
                    return RedirectToPage("/Login");
                }

                // Cargar propiedad con relaciones necesarias
                Property = await _context.Properties
                    .Include(p => p.Location)
                    .Include(p => p.Status)
                    .Include(p => p.Images.Take(1))
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId && p.OwnerId == userIdInt);

                if (Property == null)
                {
                    TempData["ErrorMessage"] = "Propiedad no encontrada.";
                    return RedirectToPage("/Profile/MyProperties");
                }

                // Actualizar estado a publicado
                await UpdatePropertyStatus(Property.PropertyId);

                // Generar código de publicación
                PublicationCode = GeneratePublicationCode(Property.PropertyId);

                // Preparar datos para la vista
                PropertyTitle = Property.Title;
                Price = Property.Price;
                Currency = Property.Currency.ToString();
                PublishedDate = HoraArgentina.Now;

                // Preparar guía de próximos pasos
                NextStepsGuide = PrepareNextStepsGuide(Property);

                // Registrar actividad de publicación
                await LogPublicationActivity(Property.PropertyId, userIdInt);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mostrar éxito de publicación para propiedad {PropertyId}", propertyId);
                TempData["ErrorMessage"] = "Error al procesar la publicación exitosa.";
                return RedirectToPage("/Profile/MyProperties");
            }
        }

        private async Task UpdatePropertyStatus(int propertyId)
        {
            var status = await _context.PropertyStatuses
                .FirstOrDefaultAsync(s => s.PropertyId == propertyId);

            if (status == null)
            {
                status = new PropertyStatus
                {
                    PropertyId = propertyId,
                    State = PropertyState.Publicado,
                    Notes = "Publicación exitosa - " + HoraArgentina.Now.ToString("dd/MM/yyyy HH:mm"),
                    UpdatedAt = HoraArgentina.Now
                };
                _context.PropertyStatuses.Add(status);
            }
            else
            {
                status.State = PropertyState.Publicado;
                status.Notes = "Publicación exitosa - " + HoraArgentina.Now.ToString("dd/MM/yyyy HH:mm");
                status.UpdatedAt = HoraArgentina.Now;
                _context.PropertyStatuses.Update(status);
            }

            await _context.SaveChangesAsync();
        }

        private string GeneratePublicationCode(int propertyId)
        {
            return $"PROP-{propertyId}-{DateTime.Now:yyyyMMdd}";
        }

        private string PrepareNextStepsGuide(Property property)
        {
            var guide = new System.Text.StringBuilder();

            guide.AppendLine("Te recomendamos:");
            guide.AppendLine("1. Revisar que todos los datos sean correctos");
            guide.AppendLine("2. Compartir tu propiedad en redes sociales");
            guide.AppendLine("3. Estar atento a consultas de interesados");
            guide.AppendLine("4. Mantener actualizado el precio y disponibilidad");

            return guide.ToString();
        }

        private async Task LogPublicationActivity(int propertyId, int userId)
        {
            // Aquí podrías registrar en una tabla de actividad/log
            var activity = new
            {
                PropertyId = propertyId,
                UserId = userId,
                Action = "PropertyPublished",
                Timestamp = HoraArgentina.Now,
                Details = $"Propiedad {propertyId} publicada exitosamente"
            };

            // Guardar en base de datos si tienes una tabla de actividades
            _logger.LogInformation("Propiedad {PropertyId} publicada por usuario {UserId}", propertyId, userId);
        }
    }
}