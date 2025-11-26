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

        [MaxLength(100)]
        public string? Neighborhood { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Province { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; } = new Property();
    }
}
