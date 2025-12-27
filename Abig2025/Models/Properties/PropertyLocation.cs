using Abig2025.Models.Location;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models.Properties
{
    public class PropertyLocation
    {
        [Key]
        public int LocationId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? Street { get; set; }

        [MaxLength(20)]
        public string? Number { get; set; }


        public int? NeighborhoodId { get; set; }
        public int? ProvinceId { get; set; }
        public int? CityId { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; } = null!;

        // Navegación
        [ForeignKey("ProvinceId")]
        public virtual Province? Province { get; set; }

        [ForeignKey("CityId")]
        public virtual City? City { get; set; }

        [ForeignKey("NeighborhoodId")]
        public virtual Neighborhood? Neighborhood { get; set; }

        // Mantener los campos de texto como respaldo
        [MaxLength(100)]
        public string? CityName { get; set; } // Si no encuentra en DB

        [MaxLength(100)]
        public string? ProvinceName { get; set; }
    }
}
