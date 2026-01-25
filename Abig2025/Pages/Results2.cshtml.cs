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

        // Nuevos parámetros de filtro
        [BindProperty(SupportsGet = true)]
        public int? Ambientes { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Banos { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PrecioMin { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PrecioMax { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Moneda { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? SuperficieTotalMin { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? SuperficieTotalMax { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? SuperficieCubiertaMin { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? SuperficieCubiertaMax { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Antiguedad { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Caracteristicas { get; set; } // Lista separada por comas

        [BindProperty(SupportsGet = true)]
        public int? Barrio { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? OrdenarPor { get; set; } = "recientes"; // Opciones: "recientes", "precio-asc", "precio-desc"

        // Parámetros de paginación
        [BindProperty(SupportsGet = true)]
        public int PaginaActual { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int TamanoPagina { get; set; } = 6; // 12 propiedades por página

        // Información adicional para la vista
        public int TotalPropiedades { get; set; }
        public int TotalPaginas { get; set; }
        public bool HayPaginaAnterior => PaginaActual > 1;
        public bool HayPaginaSiguiente => PaginaActual < TotalPaginas;
        public int IndiceInicio => (PaginaActual - 1) * TamanoPagina + 1;
        public int IndiceFin => Math.Min(PaginaActual * TamanoPagina, TotalPropiedades);

        public bool TieneFiltros => !string.IsNullOrEmpty(Operacion) ||
                                     !string.IsNullOrEmpty(Tipo) ||
                                     Dormitorios.HasValue ||
                                     Provincia.HasValue ||
                                     Ciudad.HasValue ||
                                     Ambientes.HasValue ||
                                     Banos.HasValue ||
                                     PrecioMin.HasValue ||
                                     PrecioMax.HasValue ||
                                     SuperficieTotalMin.HasValue ||
                                     SuperficieTotalMax.HasValue ||
                                     SuperficieCubiertaMin.HasValue ||
                                     SuperficieCubiertaMax.HasValue ||
                                     !string.IsNullOrEmpty(Antiguedad) ||
                                     !string.IsNullOrEmpty(Caracteristicas) ||
                                     Barrio.HasValue;

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

            // === FILTRO: Tipo de Operación ===
            if (!string.IsNullOrEmpty(Operacion))
            {
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

            // === FILTRO: Tipo de Propiedad ===
            if (!string.IsNullOrEmpty(Tipo))
            {
                if (Enum.TryParse<PropertyType>(Tipo, true, out var propertyType))
                {
                    query = query.Where(p => p.PropertyType == propertyType);
                }
            }

            // === FILTRO: Dormitorios ===
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

            // === FILTRO: Ambientes ===
            if (Ambientes.HasValue)
            {
                if (Ambientes >= 4)
                {
                    query = query.Where(p => p.MainRooms >= 4);
                }
                else
                {
                    query = query.Where(p => p.MainRooms == Ambientes);
                }
            }

            // === FILTRO: Ba?os ===
            if (Banos.HasValue)
            {
                if (Banos >= 5)
                {
                    query = query.Where(p => p.Bathrooms >= 5);
                }
                else
                {
                    query = query.Where(p => p.Bathrooms == Banos);
                }
            }

            // === FILTRO: Precio ===
            if (PrecioMin.HasValue || PrecioMax.HasValue)
            {
                // Filtrar por moneda si se especifica
                if (!string.IsNullOrEmpty(Moneda))
                {
                    if (Moneda.Equals("USD", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(p => p.Currency == CurrencyType.USD);
                    }
                    else
                    {
                        query = query.Where(p => p.Currency == CurrencyType.ARS);
                    }
                }

                if (PrecioMin.HasValue)
                {
                    query = query.Where(p => p.Price >= PrecioMin.Value);
                }

                if (PrecioMax.HasValue)
                {
                    query = query.Where(p => p.Price <= PrecioMax.Value);
                }
            }

            // === FILTRO: Ubicación ===
            if (Provincia.HasValue)
            {
                query = query.Where(p => p.Location.ProvinceId == Provincia);
            }

            if (Ciudad.HasValue)
            {
                query = query.Where(p => p.Location.CityId == Ciudad);
            }

            if (Barrio.HasValue)
            {
                query = query.Where(p => p.Location.NeighborhoodId == Barrio);
            }

            // === FILTRO: Superficie Total ===
            if (SuperficieTotalMin.HasValue)
            {
                query = query.Where(p => p.TotalArea >= SuperficieTotalMin.Value);
            }

            if (SuperficieTotalMax.HasValue)
            {
                query = query.Where(p => p.TotalArea <= SuperficieTotalMax.Value);
            }

            // === FILTRO: Superficie Cubierta ===
            if (SuperficieCubiertaMin.HasValue)
            {
                query = query.Where(p => p.CoveredArea >= SuperficieCubiertaMin.Value);
            }

            if (SuperficieCubiertaMax.HasValue)
            {
                query = query.Where(p => p.CoveredArea <= SuperficieCubiertaMax.Value);
            }

            // === FILTRO: Antigüedad ===
            if (!string.IsNullOrEmpty(Antiguedad))
            {
                switch (Antiguedad.ToLower())
                {
                    case "a estrenar":
                        query = query.Where(p => p.IsNew || p.Age == 0);
                        break;
                    case "1 a 10 a?os":
                        query = query.Where(p => p.Age >= 1 && p.Age <= 10);
                        break;
                    case "10 a 25 a?os":
                        query = query.Where(p => p.Age >= 10 && p.Age <= 25);
                        break;
                    case "25 a 50 a?os":
                        query = query.Where(p => p.Age >= 25 && p.Age <= 50);
                        break;
                    case "50 a?os o más":
                        query = query.Where(p => p.Age >= 50);
                        break;
                }
            }

            // === FILTRO: Características (Features) ===
            if (!string.IsNullOrEmpty(Caracteristicas))
            {
                var caracteristicasList = Caracteristicas.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToList();

                if (caracteristicasList.Any())
                {
                    // Para cada característica en la lista, filtrar propiedades que la contengan
                    foreach (var caracteristica in caracteristicasList)
                    {
                        query = query.Where(p => p.Features.Any(f =>
                            f.FeatureDefinition.Key == caracteristica
                        ));
                    }
                }
            }

            // Obtener total antes de ordenar
            TotalPropiedades = await query.CountAsync();

            // Calcular paginación
            TotalPaginas = (int)Math.Ceiling((double)TotalPropiedades / TamanoPagina);

            // Asegurar que la página actual sea válida
            if (PaginaActual < 1) PaginaActual = 1;
            if (PaginaActual > TotalPaginas && TotalPaginas > 0) PaginaActual = TotalPaginas;

            // ===== ORDENAR RESULTADOS =====
            switch (OrdenarPor?.ToLower())
            {
                case "precio-pesos-asc":
                    query = query.OrderBy(p =>
                        p.Currency == CurrencyType.ARS ? 0 : 1
                    ).ThenBy(p => p.Price);
                    break;

                case "precio-pesos-desc":
                    query = query.OrderBy(p =>
                        p.Currency == CurrencyType.ARS ? 0 : 1
                    ).ThenByDescending(p => p.Price);
                    break;

                case "precio-dolares-asc":
                    query = query.OrderBy(p =>
                        p.Currency == CurrencyType.USD ? 0 : 1
                    ).ThenBy(p => p.Price);
                    break;

                case "precio-dolares-desc":
                    query = query.OrderBy(p =>
                        p.Currency == CurrencyType.USD ? 0 : 1
                    ).ThenByDescending(p => p.Price);
                    break;

                case "recientes":
                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            // Aplicar paginación
            var propiedadesPaginadas = await query
                .Skip((PaginaActual - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToListAsync();

            Properties = propiedadesPaginadas;

            return Page();
        }
    }
}