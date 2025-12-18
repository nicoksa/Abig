using Abig2025.Data;
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
    }
}
