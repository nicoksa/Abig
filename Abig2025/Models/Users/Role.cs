using System.ComponentModel.DataAnnotations;

namespace Abig2025.Models.Users
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        // Relaciones
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}