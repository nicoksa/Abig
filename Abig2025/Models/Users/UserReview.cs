using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models.Users
{
    public class UserReview
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int ReviewerId { get; set; }

        [Required]
        public int ReviewedId { get; set; }

        [Range(1, 5)]
        public int Score { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ReviewerId")]
        public virtual User Reviewer { get; set; }

        [ForeignKey("ReviewedId")]
        public virtual User Reviewed { get; set; }
    }
}
