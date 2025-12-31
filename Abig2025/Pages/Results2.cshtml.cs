using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Abig2025.Data;
using Abig2025.Models.Properties;
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

        public async Task<IActionResult> OnGetAsync()
        {
            // Obtener todas las propiedades activas con sus relaciones
            Properties = await _context.Properties
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
                    .ThenInclude(f => f.FeatureDefinition)  // ¡IMPORTANTE!
                .Where(p => p.IsActive && p.Status.State == PropertyState.Publicado)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}