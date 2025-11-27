using Abig2025.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Abig2025.Models.Properties
{
    public class PropertyDraft
    {
        [Key]
        public Guid DraftId { get; set; } = Guid.NewGuid();

        public int UserId { get; set; }

        public int CurrentStep { get; set; } = 1;

        // JSON con todos los datos temporales
        public string JsonData { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; } = HoraArgentina.Now;
    }
}
