// Models/LoginAttempt.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models
{
    public class LoginAttempt
    {
        [Key]
        public int AttemptId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsSuccessful { get; set; }

        [MaxLength(45)]
        public string IpAddress { get; set; }

        [MaxLength(500)]
        public string UserAgent { get; set; }

        // Propiedad de navegación
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}