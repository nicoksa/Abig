using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models.Properties
{
    public class PropertyFeature
    {
        [Key]
        public int FeatureId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Ej: "Ambientes"

        [MaxLength(150)]
        public string? Value { get; set; } // Ej: "3"

        public int DisplayOrder { get; set; } = 0;

        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; } = new Property();
    }
}
