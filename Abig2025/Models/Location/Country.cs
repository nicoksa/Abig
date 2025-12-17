using System.ComponentModel.DataAnnotations;

namespace Abig2025.Models.Location
{
    public class Country
    {
        public int CountryId { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? Code { get; set; } // "AR", "BR", etc.

        // Navegación
        public virtual ICollection<Province> Provinces { get; set; } = new List<Province>();
    }
}
