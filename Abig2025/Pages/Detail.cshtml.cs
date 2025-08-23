using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Abig2025.Pages
{
    public class DetailModel : PageModel
    {
        public Propiedad Propiedad { get; set; }

        public void OnGet(int id)
        {
            // Datos de ejemplo
            Propiedad = new Propiedad
            {
                Id = id,
                Titulo = "Casa en alquiler 3 ambientes con cochera",
                Ubicacion = "9 de Julio al 900, San Fernando",
                Precio = 1500,
                Ambientes = 3,
                Banios = 1,
                Dormitorios = 2,
                SuperficieTotal = 90,
                Tipo = "Casa",
                Antiguedad = 10,
                ImagenPrincipal = "/images/prop3.jpg",
                Galeria = new List<string>
            {
                "/images/prop5.jpg",
                "/images/prop6.jpg",
                "/images/prop7.jpg"
            },
                
                UrlMapa = "https://www.google.com/maps/embed?...",
                Descripcion = "Casa en San Fernando, luminoso, con jardín y cochera. Excelente ubicación, " + 
                "a 5 cuadras del centro. "

            };
        }
    }

    public class Propiedad
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Ubicacion { get; set; }
        public decimal Precio { get; set; }
        public int Ambientes { get; set; }
        public int Banios { get; set; }
        public int Dormitorios { get; set; }
        public int SuperficieTotal { get; set; }
        public string Tipo { get; set; }
        public int Antiguedad { get; set; }
        public string ImagenPrincipal { get; set; }
        public List<string> Galeria { get; set; }
        
        public string UrlMapa { get; set; }
        public string Descripcion { get; set; }
    }
}
