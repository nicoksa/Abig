using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models.Users
{
    public class UserProfile
    {
        [Key]
        public int UserProfileId { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(20)]
        public string? Dni { get; set; } 

        [MaxLength(20)]
        public string? Phone { get; set; }  

        [MaxLength(255)]
        public string? Address { get; set; }  

        [MaxLength(100)]
        public string? City { get; set; }  

        [MaxLength(100)]
        public string? Province { get; set; }  

        [MaxLength(100)]
        public string? Country { get; set; }  

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!; 
    }
}
