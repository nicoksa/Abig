using System.ComponentModel.DataAnnotations;

namespace Abig2025.Models.Subscriptions
{
    public class SubscriptionPlan
    {
        [Key]
        public int PlanId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // Gratuito, Plata, Oro, Platino

        public decimal Price { get; set; }

        public int DurationDays { get; set; }

        public int MaxPublications { get; set; } // -1 si es ilimitado

        public bool IncludesContractManagement { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
