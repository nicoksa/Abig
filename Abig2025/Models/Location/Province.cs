// Models/Location/Province.cs
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;

namespace Abig2025.Models.Location
{
    public class Province
    {
        public int ProvinceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Code { get; set; } // Ej: "BA" para Buenos Aires

        public int CountryId { get; set; } = 1; // Argentina por defecto

        public bool isActive { get; set; } = true;

        // Navegación
        public virtual ICollection<City> Cities { get; set; } = new List<City>();
        public virtual Country Country { get; set; } = null!;
    }
}