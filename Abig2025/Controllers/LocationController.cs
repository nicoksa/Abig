using Abig2025.Data;
using Abig2025.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Abig2025.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.Provinces
                .OrderBy(p => p.Name)
                .Select(p => new { p.ProvinceId, p.Name })
                .ToListAsync();

            return Ok(provinces);
        }

        [HttpGet("provinces/{provinceId}/cities")]
        public async Task<IActionResult> GetCities(int provinceId)
        {
            var cities = await _context.Cities
                .Where(c => c.ProvinceId == provinceId)
                .OrderBy(c => c.Name)
                .Select(c => new { c.CityId, c.Name })
                .ToListAsync();

            return Ok(cities);
        }

        [HttpGet("cities/{cityId}/neighborhoods")]
        public async Task<IActionResult> GetNeighborhoods(int cityId)
        {
            var neighborhoods = await _context.Neighborhoods
                .Where(n => n.CityId == cityId)
                .OrderBy(n => n.Name)
                .Select(n => new { n.NeighborhoodId, n.Name })
                .ToListAsync();

            return Ok(neighborhoods);
        }


        [HttpGet("search")]
        public async Task<IActionResult> SearchLocations(string query, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Ok(new List<UbicacionResultDto>());

            var normalizedQuery = query.ToLower().Trim();
            var results = new List<UbicacionResultDto>();

            // 1. Buscar provincias
            var provinces = await _context.Provinces
                .Where(p => p.isActive && p.Name.ToLower().Contains(normalizedQuery))
                .OrderBy(p => p.Name)
                .Take(limit)
                .Select(p => new UbicacionResultDto
                {
                    Id = p.ProvinceId,
                    Nombre = p.Name,
                    Tipo = "Provincia",
                    TipoId = "provincia",
                    ProvinciaId = p.ProvinceId,
                    CiudadId = null,
                    ProvinciaNombre = p.Name,
                    DisplayText = p.Name
                })
                .ToListAsync();

            results.AddRange(provinces);

            // 2. Buscar ciudades (solo si necesitamos más resultados)
            if (results.Count < limit)
            {
                var cities = await _context.Cities
                    .Where(c => c.isActive && c.Name.ToLower().Contains(normalizedQuery))
                    .Include(c => c.Province)
                    .OrderBy(c => c.Name)
                    .Take(limit - results.Count)
                    .Select(c => new UbicacionResultDto
                    {
                        Id = c.CityId,
                        Nombre = c.Name,
                        Tipo = "Ciudad",
                        TipoId = "ciudad",
                        ProvinciaId = c.ProvinceId,
                        CiudadId = c.CityId,
                        ProvinciaNombre = c.Province.Name,
                        CiudadNombre = c.Name,
                        DisplayText = $"{c.Name}, {c.Province.Name}"
                    })
                    .ToListAsync();

                results.AddRange(cities);
            }

            // 3. Buscar barrios (solo si necesitamos más resultados)
            if (results.Count < limit)
            {
                var neighborhoods = await _context.Neighborhoods
                    .Where(n => n.isActive && n.Name.ToLower().Contains(normalizedQuery))
                    .Include(n => n.City)
                        .ThenInclude(c => c.Province)
                    .OrderBy(n => n.Name)
                    .Take(limit - results.Count)
                    .Select(n => new UbicacionResultDto
                    {
                        Id = n.NeighborhoodId,
                        Nombre = n.Name,
                        Tipo = "Barrio",
                        TipoId = "barrio",
                        ProvinciaId = n.City.ProvinceId,
                        CiudadId = n.CityId,
                        ProvinciaNombre = n.City.Province.Name,
                        CiudadNombre = n.City.Name,
                        DisplayText = $"{n.Name}, {n.City.Name}, {n.City.Province.Name}"
                    })
                    .ToListAsync();

                results.AddRange(neighborhoods);
            }

            return Ok(results);
        }


    }
}
