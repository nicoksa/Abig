using Abig2025.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abig2025.Helpers;

namespace Abig2025.Models.Subscriptions
{
    public class UserSubscription
    {
        [Key]
        public int SubscriptionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PlanId { get; set; }

        public DateTime StartDate { get; set; } = HoraArgentina.Now;

        public DateTime EndDate { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("PlanId")]
        public virtual SubscriptionPlan Plan { get; set; }
    }

}
