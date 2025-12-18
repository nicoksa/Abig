// Models/Location/City.cs
using System.ComponentModel.DataAnnotations;

namespace Abig2025.Models.Location
{
    public class City
    {
        public int CityId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int ProvinceId { get; set; }

        [MaxLength(20)]
        public string? PostalCodePrefix { get; set; }

        public bool isActive { get; set; } = true;  

        // Navegación
        public virtual Province Province { get; set; } = null!;

        public virtual ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
    }
}
