using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models
{
    public class Property
    {
        [Key]
        public int PropertyId { get; set; }

        [Required]
        public int OwnerId { get; set; } // usuario que la publicó

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public decimal Price { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        public string Category { get; set; } // Casa, Dpto, Campo, etc.

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; }

        public virtual ICollection<Favorite> Favorites { get; set; }
    }

}
