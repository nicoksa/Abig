using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models
{
    public class UserProfile
    {
        [Key]
        public int UserProfileId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Dni { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        // Relación
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

}
