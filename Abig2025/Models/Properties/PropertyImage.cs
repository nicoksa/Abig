using Abig2025.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abig2025.Models.Properties
{
    public class PropertyImage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsMain { get; set; } = false;

        public long FileSize { get; set; } // bytes

        public DateTime UploadedAt { get; set; } = HoraArgentina.Now;

        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; } = new Property();
    }
}

