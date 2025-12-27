using Abig2025.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models.Properties
{
    public enum PropertyState
    {
        Borrador = 0,
        Pendiente = 1,
        Publicado = 2,
        Pausado = 3,
        Rechazado = 4
    }

    public class PropertyStatus
    {
        [Key]
        public int StatusId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public PropertyState State { get; set; } = PropertyState.Borrador;

        public DateTime UpdatedAt { get; set; } = HoraArgentina.Now;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; } = null!;
    }
}
