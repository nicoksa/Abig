using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Abig2025.Models.ViewModels
{
    public class PostStep2ViewModel
    {
        public string? VideoUrl { get; set; }

        public List<IFormFile> Imagenes { get; set; } = new();
    }
}
