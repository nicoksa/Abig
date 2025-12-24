using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models.Properties
{
    public class PropertyFeature
    {
        [Key]
        public int PropertyFeatureId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int FeatureDefinitionId { get; set; }

        [MaxLength(150)]
        public string? Value { get; set; }

        [ForeignKey(nameof(PropertyId))]
        public Property Property { get; set; } = null!;

        [ForeignKey(nameof(FeatureDefinitionId))]
        public FeatureDefinition FeatureDefinition { get; set; } = null!;
    }
}