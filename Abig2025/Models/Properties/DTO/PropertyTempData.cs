using Abig2025.Helpers;
using Abig2025.Models.Properties.Enums;
using System.ComponentModel.DataAnnotations;
using Abig2025.ModelBinders; 
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Abig2025.Models.DTO
{
    public class PropertyTempData
    {
        // PASO 1 – Operación y tipo
        [JsonPropertyName("operationType")]
        public OperationType OperationType { get; set; }

        [JsonPropertyName("propertyType")]
        public PropertyType PropertyType { get; set; }

        [JsonPropertyName("subtype")]
        public string? Subtype { get; set; }

        // Título + descripción
        [Required(ErrorMessage = "El título es obligatorio")]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        // Precio
        [Required(ErrorMessage = "El precio es obligatorio")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("currency")]
        public CurrencyType Currency { get; set; }

        // Expensas
        [JsonPropertyName("expenses")]
        public decimal? Expenses { get; set; }

        [JsonPropertyName("expensesCurrency")]
        public CurrencyType? ExpensesCurrency { get; set; }

        // PASO 2 – Ubicación
        [JsonPropertyName("province")]
        public string? Province { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("neighborhood")]
        public string? Neighborhood { get; set; }


        [JsonPropertyName("provinceId")]
        public int? ProvinceId { get; set; }

        [JsonPropertyName("cityId")]
        public int? CityId { get; set; }

        [JsonPropertyName("neighborhoodId")]
        public int? NeighborhoodId { get; set; }


        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("number")]
        public string? Number { get; set; }

        [JsonPropertyName("postalCode")]
        public string? PostalCode { get; set; }

        // Mapa
        [ModelBinder(BinderType = typeof(DecimalModelBinder))]
        [JsonPropertyName("latitude")]
        public decimal? Latitude { get; set; }

        [ModelBinder(BinderType = typeof(DecimalModelBinder))]
        [JsonPropertyName("longitude")]
        public decimal? Longitude { get; set; }

        // PASO 3 – Características principales

        [JsonPropertyName("mainrooms")]
        public int? MainRooms { get; set; }             // Ambientes

        [JsonPropertyName("bedrooms")]
        public int? Bedrooms { get; set; }              // Dormitorios

        [JsonPropertyName("bathrooms")]
        public int? Bathrooms { get; set; }             // Baños

        [JsonPropertyName("parkingSpaces")]
        public int? ParkingSpaces { get; set; }         // Cocheras


        [JsonPropertyName("coveredArea")]
        public double? CoveredArea { get; set; }        // Sup cubierta

        [JsonPropertyName("totalArea")]
        public double? TotalArea { get; set; }          // Sup total


        [JsonPropertyName("age")]
        public int? Age { get; set; }                   // Antigüedad

        [JsonPropertyName("isUnderConstruction")]
        public bool IsUnderConstruction { get; set; }   // En construcción

        [JsonPropertyName("isNew")]
        public bool IsNew { get; set; }                 // A estrenar

        // PASO 3 extra – Features dinámicos
        public List<PropertyFeatureTemp> Features { get; set; } = new();
        public List<FieldFeatureTemp> FieldFeatures { get; set; } = new();


        // PASO 4 – Imágenes temporales
        public List<TempImageInfo> TempImages { get; set; } = new();
        public string? VideoUrl { get; set; }
    }

    public class PropertyFeatureTemp
    {
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }
    }
    public class FieldFeatureTemp
    {
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }
    }
    public class TempImageInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime UploadedAt { get; set; } = HoraArgentina.Now;
    }
}
