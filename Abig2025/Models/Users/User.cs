// Models/User.cs
using Abig2025.Models.Properties;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models.Users
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordSalt { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        public bool IsEmailVerified { get; set; } = false;

        public Guid? EmailVerificationToken { get; set; }

        public DateTime? EmailVerificationTokenExpiry { get; set; }

        public Guid? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }

        public bool IsActive { get; set; } = true;

        // Relaciones
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<LoginAttempt> LoginAttempts { get; set; }
        public virtual ICollection<Property> Properties { get; set; }
    }
}
