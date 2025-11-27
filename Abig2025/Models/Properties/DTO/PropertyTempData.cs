using Abig2025.Models.Properties.Enums;

namespace Abig2025.Models.DTO
{
    public class PropertyTempData
    {
        // PASO 1 – Operación y tipo
        public OperationType OperationType { get; set; }
        public PropertyType PropertyType { get; set; }
        public string? Subtype { get; set; }

        // Título + descripción
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Precio
        public decimal Price { get; set; }
        public CurrencyType Currency { get; set; }

        // Expensas
        public decimal? Expenses { get; set; }
        public CurrencyType? ExpensesCurrency { get; set; }

        // PASO 2 – Ubicación
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Neighborhood { get; set; }
        public string? Street { get; set; }
        public string? Number { get; set; }
        public string? PostalCode { get; set; }

        // Mapa
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // PASO 3 – Características principales
        public int? MainRooms { get; set; }             // Ambientes
        public int? Bedrooms { get; set; }              // Dormitorios
        public int? Bathrooms { get; set; }             // Baños
        public int? ParkingSpaces { get; set; }         // Cocheras

        public double? CoveredArea { get; set; }        // Sup cubierta
        public double? TotalArea { get; set; }          // Sup total

        public int? Age { get; set; }                   // Antigüedad
        public bool IsUnderConstruction { get; set; }   // En construcción
        public bool IsNew { get; set; }                 // A estrenar

        // PASO 3 extra – Features dinámicos
        public List<PropertyFeatureTemp> Features { get; set; } = new();

        // PASO 4 – Imágenes temporales
        public List<string> TempImageNames { get; set; } = new();
    }

    public class PropertyFeatureTemp
    {
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }
    }
}
