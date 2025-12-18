using System.ComponentModel.DataAnnotations;

namespace Abig2025.Models.Location
{
    public class Neighborhood
    {
        public int NeighborhoodId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CityId { get; set; }

        public bool isActive { get; set; } = true;

        [MaxLength(20)]
        public string? PostalCodePrefix { get; set; }
        public City City { get; set; } = null!;
    }
}
