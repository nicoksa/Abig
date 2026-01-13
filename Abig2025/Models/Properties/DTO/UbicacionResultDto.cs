
namespace Abig2025.Models.DTOs
{
    public class UbicacionResultDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string TipoId { get; set; } = string.Empty;
        public int ProvinciaId { get; set; }
        public int? CiudadId { get; set; }
        public string DisplayText { get; set; } = string.Empty;

        // Propiedades adicionales para contexto
        public string? ProvinciaNombre { get; set; }
        public string? CiudadNombre { get; set; }
    }
}
