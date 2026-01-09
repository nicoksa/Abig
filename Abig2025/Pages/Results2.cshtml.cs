using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Abig2025.Data;
using Abig2025.Models.Properties;
using Abig2025.Models.Properties.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Abig2025.Pages
{
    public class Results2Model : PageModel
    {
        private readonly AppDbContext _context;

        public Results2Model(AppDbContext context)
        {
            _context = context;
        }

        public List<Property> Properties { get; set; } = new List<Property>();

        // Parámetros de búsqueda desde el Index
        [BindProperty(SupportsGet = true)]
        public string? Operacion { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Tipo { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Dormitorios { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Provincia { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Ciudad { get; set; }

        // Información adicional para la vista
        public int TotalPropiedades { get; set; }
        public bool TieneFiltros => !string.IsNullOrEmpty(Operacion) ||
                                     !string.IsNullOrEmpty(Tipo) ||
                                     Dormitorios.HasValue ||
                                     Provincia.HasValue ||
                                     Ciudad.HasValue;

        public async Task<IActionResult> OnGetAsync()
        {
            // Query base con todas las relaciones necesarias
            var query = _context.Properties
                .Include(p => p.Owner)
                .Include(p => p.Location)
                    .ThenInclude(l => l.Province)
                .Include(p => p.Location)
                    .ThenInclude(l => l.City)
                .Include(p => p.Location)
                    .ThenInclude(l => l.Neighborhood)
                .Include(p => p.Images)
                .Include(p => p.Status)
                .Include(p => p.Features)
                    .ThenInclude(f => f.FeatureDefinition)
                .Where(p => p.IsActive && p.Status.State == PropertyState.Publicado)
                .AsQueryable();

            // Aplicar filtros desde el Index
            if (!string.IsNullOrEmpty(Operacion))
            {
                // Normalizar el valor para comparar con el enum
                if (Operacion.Equals("Alquiler", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.OperationType == OperationType.Alquiler);
                }
                else if (Operacion.Equals("AlquilerTemporal", StringComparison.OrdinalIgnoreCase) ||
                         Operacion.Equals("Temporario", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.OperationType == OperationType.AlquilerTemporal);
                }
            }

            if (!string.IsNullOrEmpty(Tipo))
            {
                if (Enum.TryParse<PropertyType>(Tipo, true, out var propertyType))
                {
                    query = query.Where(p => p.PropertyType == propertyType);
                }
            }

            if (Dormitorios.HasValue)
            {
                if (Dormitorios >= 5)
                {
                    query = query.Where(p => p.Bedrooms >= 5);
                }
                else
                {
                    query = query.Where(p => p.Bedrooms == Dormitorios);
                }
            }

            if (Provincia.HasValue)
            {
                query = query.Where(p => p.Location.ProvinceId == Provincia);
            }

            if (Ciudad.HasValue)
            {
                query = query.Where(p => p.Location.CityId == Ciudad);
            }

            // Obtener total antes de ordenar
            TotalPropiedades = await query.CountAsync();

            // Ordenar por fecha de creación (más recientes primero)
            Properties = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}