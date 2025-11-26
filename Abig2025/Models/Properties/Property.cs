using Abig2025.Models.Users;
using Abig2025.Models.Properties.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abig2025.Helpers;

namespace Abig2025.Models.Properties
{
    public class Property
    {
        [Key]
        public int PropertyId { get; set; }

        [Required]
        public int OwnerId { get; set; }

        // Información básica
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal Price { get; set; }

        public CurrencyType Currency { get; set; } = CurrencyType.ARS;

        // Tipos de operación y propiedad
        public OperationType OperationType { get; set; } // Venta / Alquiler
        public PropertyType PropertyType { get; set; }   // Casa, Dpto, PH, etc.

        // Fechas y estado
        public DateTime CreatedAt { get; set; } = HoraArgentina.Now;
        public bool IsActive { get; set; } = true;

        // Relaciones
        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; } = new User();

        public virtual PropertyLocation Location { get; set; } = new PropertyLocation();
        public virtual PropertyStatus Status { get; set; } = new PropertyStatus();

        public virtual ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public virtual ICollection<PropertyFeature> Features { get; set; } = new List<PropertyFeature>();

        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
