// Models/Properties/PropertyPublication.cs
using Abig2025.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models.Properties
{
    public class PropertyPublication
    {
        [Key]
        public int PublicationId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PlanId { get; set; }

        public DateTime PublishedAt { get; set; } = HoraArgentina.Now;

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual Users.User User { get; set; } = null!;

        [ForeignKey("PlanId")]
        public virtual Subscriptions.SubscriptionPlan Plan { get; set; } = null!;
    }
}