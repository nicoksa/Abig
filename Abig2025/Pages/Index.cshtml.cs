using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Abig2025.Data;
using Abig2025.Models.Location;

namespace Abig2025.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AppDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public List<Province> Provinces { get; set; } = new List<Province>();
        public List<City> Cities { get; set; } = new List<City>();

        public async Task OnGetAsync()
        {
            // Cargar provincias activas para el formulario
            Provinces = await _context.Provinces
                .Where(p => p.isActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        // Endpoint para obtener ciudades por provincia (AJAX)
        public async Task<JsonResult> OnGetCitiesByProvinceAsync(int provinceId)
        {
            var cities = await _context.Cities
                .Where(c => c.ProvinceId == provinceId && c.isActive)
                .OrderBy(c => c.Name)
                .Select(c => new { c.CityId, c.Name })
                .ToListAsync();

            return new JsonResult(cities);
        }
    }
}